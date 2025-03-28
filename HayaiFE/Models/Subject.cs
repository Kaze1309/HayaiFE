namespace HayaiFE.Models
{
    public class Subject
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public Subject(string code, string name, string type)
        {
            Code = code;
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Code}_{Name}_{Type}";
        }
    }
}
