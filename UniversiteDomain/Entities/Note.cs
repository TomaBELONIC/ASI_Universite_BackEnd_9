namespace UniversiteDomain.Entities
{
    public class Note
    {
        // Association vers Etudiant et Ue
        public Etudiant Etudiant { get; set; } = null!;
        public Ue Ue { get; set; } = null!;
        
        // C'est la clé primaire de cette entité, ces deux attributs
        // sont des clés étrangères
        public long IdEtudiant { get; set; }
        public long IdUe { get; set; }

        /*
         * "null!" c'est pour dire : non-nullable
         * On va initialiser Etudiant et Ue à null au début mais on va forcément les initialiser plus tard
         *  car la Note sans ces deux entités n'a pas de sens
         */
        
        // valeur moyenne finale de l'UE (entre 0 et 20)
        public float Valeur { get; set; }

        public override string ToString()
        {
            return $"Note associée à l'étudiant : Etudiant={Etudiant.NumEtud}, Ue={Ue.NumeroUe}, Valeur={Valeur}";
        }
    }
}