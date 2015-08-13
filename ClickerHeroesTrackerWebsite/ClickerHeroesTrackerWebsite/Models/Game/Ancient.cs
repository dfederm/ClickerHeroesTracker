namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;

    public class Ancient
    {
        private static Dictionary<int, Ancient> ancients = new Dictionary<int, Ancient>();

        public static readonly Ancient Solomon = new Ancient(
            id: 3,
            name: "Solomon",
            title: "Ancient of Wisdom",
            description: "undefined",
            maxLevel: -1,
            power: 1.5);
        public static readonly Ancient Libertas = new Ancient(
            id: 4,
            name: "Libertas",
            title: "Ancient of Freedom",
            description: "undefined",
            maxLevel: -1,
            power: 1);
        public static readonly Ancient Siyalatas = new Ancient(
            id: 5,
            name: "Siyalatas",
            title: "Ancient of Abandon",
            description: "undefined",
            maxLevel: -1,
            power: 1);
        public static readonly Ancient Khrysos = new Ancient(
            id: 6,
            name: "Khrysos",
            title: "Ancient of Inheritance",
            description: "undefined",
            maxLevel: 10,
            power: 1.5);
        public static readonly Ancient Thusia = new Ancient(
            id: 7,
            name: "Thusia",
            title: "Ancient of Vaults",
            description: "undefined",
            maxLevel: -1,
            power: 1.5);
        public static readonly Ancient Mammon = new Ancient(
            id: 8,
            name: "Mammon",
            title: "Ancient of Greed",
            description: "undefined",
            maxLevel: -1,
            power: 1);
        public static readonly Ancient Mimzee = new Ancient(
            id: 9,
            name: "Mimzee",
            title: "Ancient of Riches",
            description: "undefined",
            maxLevel: -1,
            power: 1);
        public static readonly Ancient Pluto = new Ancient(
            id: 10,
            name: "Pluto",
            title: "Ancient of Wealth",
            description: "undefined",
            maxLevel: -1,
            power: 1);
        public static readonly Ancient Dogcog = new Ancient(
            id: 11,
            name: "Dogcog",
            title: "Ancient of Thrift",
            description: "undefined",
            maxLevel: 25,
            power: 1);
        public static readonly Ancient Fortuna = new Ancient(
            id: 12,
            name: "Fortuna",
            title: "Ancient of Chance",
            description: "undefined",
            maxLevel: 40,
            power: 1);
        public static readonly Ancient Atman = new Ancient(
            id: 13,
            name: "Atman",
            title: "Ancient of Souls",
            description: "undefined",
            maxLevel: 25,
            power: 1.5);
        public static readonly Ancient Dora = new Ancient(
            id: 14,
            name: "Dora",
            title: "Ancient of Discovery",
            description: "undefined",
            maxLevel: 50,
            power: 1);
        public static readonly Ancient Bhaal = new Ancient(
            id: 15,
            name: "Bhaal",
            title: "Ancient of Murder",
            description: "undefined",
            maxLevel: -1,
            power: 1);
        public static readonly Ancient Morgulis = new Ancient(
            id: 16,
            name: "Morgulis",
            title: "Ancient of Death",
            description: "undefined",
            maxLevel: -1,
            power: 0);
        public static readonly Ancient Chronos = new Ancient(
            id: 17,
            name: "Chronos",
            title: "Ancient of Time",
            description: "undefined",
            maxLevel: -1,
            power: 1.5);
        public static readonly Ancient Bubos = new Ancient(
            id: 18,
            name: "Bubos",
            title: "Ancient of Diseases",
            description: "undefined",
            maxLevel: 25,
            power: 1);
        public static readonly Ancient Fragsworth = new Ancient(
            id: 19,
            name: "Fragsworth",
            title: "Ancient of Wrath",
            description: "undefined",
            maxLevel: -1,
            power: 1);
        public static readonly Ancient Vaagur = new Ancient(
            id: 20,
            name: "Vaagur",
            title: "Ancient of Impatience",
            description: "undefined",
            maxLevel: 15,
            power: 1);
        public static readonly Ancient Kumawakamaru = new Ancient(
            id: 21,
            name: "Kumawakamaru",
            title: "Ancient of Shadows",
            description: "undefined",
            maxLevel: 5,
            power: 1);
        public static readonly Ancient Chawedo = new Ancient(
            id: 22,
            name: "Chawedo",
            title: "Ancient of Agitation",
            description: "undefined",
            maxLevel: 30,
            power: 1);
        public static readonly Ancient Hecatoncheir = new Ancient(
            id: 23,
            name: "Hecatoncheir",
            title: "Ancient of Wallops",
            description: "undefined",
            maxLevel: 30,
            power: 1);
        public static readonly Ancient Berserker = new Ancient(
            id: 24,
            name: "Berserker",
            title: "Ancient of Rage",
            description: "undefined",
            maxLevel: 30,
            power: 1);
        public static readonly Ancient Sniperino = new Ancient(
            id: 25,
            name: "Sniperino",
            title: "Ancient of Accuracy",
            description: "undefined",
            maxLevel: 30,
            power: 1);
        public static readonly Ancient Kleptos = new Ancient(
            id: 26,
            name: "Kleptos",
            title: "Ancient of Thieves",
            description: "undefined",
            maxLevel: 30,
            power: 1);
        public static readonly Ancient Energon = new Ancient(
            id: 27,
            name: "Energon",
            title: "Ancient of Battery Life",
            description: "undefined",
            maxLevel: 30,
            power: 1);
        public static readonly Ancient Argaiv = new Ancient(
            id: 28,
            name: "Argaiv",
            title: "Ancient of Enhancement",
            description: "undefined",
            maxLevel: -1,
            power: 1);
        public static readonly Ancient Juggernaut = new Ancient(
            id: 29,
            name: "Juggernaut",
            title: "Ancient of Momentum",
            description: "undefined",
            maxLevel: -1,
            power: 1.5);
        public static readonly Ancient Iris = new Ancient(
            id: 30,
            name: "Iris",
            title: "Ancient of Vision",
            description: "undefined",
            maxLevel: -1,
            power: 1.5);
        public static readonly Ancient Revolc = new Ancient(
            id: 31,
            name: "Revolc",
            title: "Ancient of Luck",
            description: "undefined",
            maxLevel: 15,
            power: 0);

        private Ancient(
            int id,
            string name,
            string title,
            string description,
            int maxLevel,
            double power)
        {
            this.Id = id;
            this.Name = name;
            this.Title = title;
            this.Description = description;
            this.MaxLevel = maxLevel;
            this.Power = power;

            // Add itself to the static collection
            ancients.Add(this.Id, this);
        }

        public static IEnumerable<Ancient> All
        {
            get
            {
                return ancients.Values;
            }
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public int MaxLevel { get; private set; }

        public double Power { get; private set; }

        public static Ancient Get(int id)
        {
            Ancient ancient;
            return ancients.TryGetValue(id, out ancient) ? ancient : null;
        }
    }
}