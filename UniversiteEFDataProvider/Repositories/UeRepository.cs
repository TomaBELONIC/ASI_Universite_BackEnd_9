using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace UniversiteEFDataProvider.Repositories;

public class UeRepository(UniversiteDbContext context) : Repository<Ue>(context), IUeRepository
{
    public async Task<Ue> AffecterParcoursAsync(long idUe, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Ues);
        Ue ue = (await Context.Ues.FindAsync(idUe))!;
        
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
        
        ue.EnseigneeDans!.Add(p);
        await Context.SaveChangesAsync();
        return ue;
    }
    
    public async Task<Ue> AffecterParcoursAsync(Ue ue, Parcours parcours)
    {
        return await AffecterParcoursAsync(ue.Id, parcours.Id); 
    }

    public long[] GetUeParcours(long idUe)
    {
        ArgumentNullException.ThrowIfNull(Context.Ues);
        
        var parcoursIds = Context.Ues
            .Where(u => u.Id == idUe)
            .SelectMany(u => u.EnseigneeDans.Select(p => p.Id))
            .ToArray();
        
        // là on passe par parcours
        // var parcoursIds2 = Context.Parcours
        //     .Where(p => p.UesEnseignees.Any(u => u.Id == idUe))
        //     .Select(p => p.Id)
        //     .ToArray();
        
        return parcoursIds;
    }
    
    public void CreerLeFichierNotesPourCetteUe(Ue ue, List<DonneesFichierCsv> infos)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            NewLine = Environment.NewLine
        };

        var intitule = ue.Intitule.Replace(' ', '_');
        var numeroUe = ue.NumeroUe;
        var nomDuFichier = "NotesPourUe__" + intitule + ".csv";

        var mettreDansDossier = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", ".."));
        
        mettreDansDossier = Path.Combine(mettreDansDossier, "Notes_Ues");
        Directory.CreateDirectory(mettreDansDossier);
        
        var csvPath = Path.Combine(mettreDansDossier, nomDuFichier);
        using var streamWriter = new StreamWriter(csvPath);
        using var csvWriter = new CsvWriter(streamWriter, config);
        
        // Ligne1 : "NumeroUe,Intitule"
        csvWriter.WriteField("NumeroUe");
        csvWriter.WriteField("Intitule");
        csvWriter.NextRecord();

        // info de l'UE
        csvWriter.WriteField(numeroUe);
        csvWriter.WriteField(intitule);
        csvWriter.NextRecord();

        // 2 lignes vides
        csvWriter.NextRecord();
        csvWriter.NextRecord();

        // en-tête des colonnes des étudiants NumEtud, Nom, Prenom et pour la note de l'UE
        csvWriter.WriteField("NumEtud");
        csvWriter.WriteField("Nom");
        csvWriter.WriteField("Prenom");
        csvWriter.WriteField("Note");
        csvWriter.NextRecord();

        // les lignes de données
        foreach (var info in infos)
        {
            csvWriter.WriteRecord(info);
            csvWriter.NextRecord();
        }
    }

    public (List<DonneesFichierCsv> Donnees, List<string> Erreurs) LireLeFichierNotesPourCetteUe(Ue ue, IFormFile fichierImporte)
    {
        if (ue == null) throw new ArgumentNullException(nameof(ue));
        if (fichierImporte == null) throw new ArgumentNullException(nameof(fichierImporte));
        if (fichierImporte.Length == 0) throw new ArgumentException("Le fichier importé est vide.", nameof(fichierImporte));

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };

        var resultList = new List<DonneesFichierCsv>();
        var errors = new List<string>();

        // Ouvrir le stream fourni par IFormFile
        using var stream = fichierImporte.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        // sauter les 2 premières lignes et trouver l'en-tête "NumEtud"
        if (!csv.Read()) throw new InvalidDataException("Fichier CSV vide.");
        if (!csv.Read()) throw new InvalidDataException("Fichier CSV incomplet (ligne UE manquante).");

        // trouver la ligne d'en-tête des étudiants
        bool headerFound = false;
        while (csv.Read())
        {
            if (csv.Parser.Record != null && csv.Parser.Record.Length > 0 &&
                string.Equals(csv.GetField(0), "NumEtud", StringComparison.OrdinalIgnoreCase))
            {
                csv.ReadHeader();
                headerFound = true;
                break;
            }
        }

        if (!headerFound)
            throw new InvalidDataException("En-tête 'NumEtud' introuvable dans le fichier CSV.");

        // lecture ligne par ligne et validation
        while (csv.Read())
        {
            // lire champs textuels (TryGetField pour éviter exceptions)
            csv.TryGetField("NumEtud", out string numEtud);
            csv.TryGetField("Nom", out string nom);
            csv.TryGetField("Prenom", out string prenom);

            // lire note brute (peut être vide)
            csv.TryGetField("Note", out string noteRaw);

            decimal? parsedNote = null;

            if (!string.IsNullOrWhiteSpace(noteRaw))
            {
                // normaliser séparateur décimal : accepter ',' ou '.'
                var norm = noteRaw.Trim().Replace(',', '.');

                // tenter parse avec InvariantCulture
                // if (float.TryParse(norm, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                // {
                //     // validation bornes
                //     if (val < 0f || val > 20f)
                //     {
                //         errors.Add($"Ligne {csv.Parser.Row}: note hors bornes ({val}) pour l'étudiant {numEtud ?? "(inconnu)"}.");
                //         parsedNote = null;
                //     }
                //     else
                //     {
                //         parsedNote = val;
                //     }
                // }
                if (decimal.TryParse(norm, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal dval))
                {
                    dval = decimal.Round(dval, 1, MidpointRounding.AwayFromZero);
                    if (dval < 0 || dval > 20)
                    {
                        errors.Add($"Ligne {csv.Parser.Row}: note hors bornes ({dval}) pour l'étudiant {numEtud ?? "(inconnu)"}.");
                        parsedNote = null;
                    }
                    parsedNote = dval;
                }
                else
                {
                    errors.Add($"Ligne {csv.Parser.Row}: note invalide '{noteRaw}' pour l'étudiant {numEtud ?? "(inconnu)"}.");
                    parsedNote = null;
                }
            }

            var entry = new DonneesFichierCsv
            {
                NumEtud = numEtud ?? string.Empty,
                Nom = nom ?? string.Empty,
                Prenom = prenom ?? string.Empty,
                Note = parsedNote
            };

            resultList.Add(entry);
        }

        return (resultList, errors);
    }
}