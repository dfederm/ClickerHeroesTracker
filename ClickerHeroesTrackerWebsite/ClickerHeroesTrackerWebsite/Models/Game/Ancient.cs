namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;

    public class Ancient
    {
        private static Dictionary<int, Ancient> ancients = new Dictionary<int, Ancient>();

        // Populated by: http://s3-us-west-2.amazonaws.com/clickerheroes/ancientssoul.html
        // var out = ""; for (var key in Ancients) { var ancient = Ancients[key]; out += "public static readonly Ancient " + ancient.name.substring(0, ancient.name.indexOf(',')) + " = new Ancient(\n    id: "+ancient.id+",\n    maxLevel: "+(ancient.maxlvl ? ancient.maxlvl : -1)+",\n    name: \"" + ancient.name.substring(0, ancient.name.indexOf(',')) + "\",\n    title: \"" + ancient.name.substring(ancient.name.indexOf(',') + 2) + "\",\n    description: \"" + ancient.desc + "\");\n" };
        // TODO: move to SQL
        public static readonly Ancient Solomon = new Ancient(
            id: 3,
            maxLevel: -1,
            name: "Solomon",
            title: "Ancient of Wisdom",
            description: "+5% to 1% Primal hero souls");
        public static readonly Ancient Libertas = new Ancient(
            id: 4,
            maxLevel: -1,
            name: "Libertas",
            title: "Ancient of Freedom",
            description: "+25% to +15% idle Gold");
        public static readonly Ancient Siyalatas = new Ancient(
            id: 5,
            maxLevel: -1,
            name: "Siyalatas",
            title: "Ancient of Abandon",
            description: "+25% to +15% idle DPS");
        public static readonly Ancient Khrysos = new Ancient(
            id: 6,
            maxLevel: 10,
            name: "Khrysos",
            title: "Ancient of Inheritance",
            description: "+50 starting Gold when Ascending");
        public static readonly Ancient Thusia = new Ancient(
            id: 7,
            maxLevel: 10,
            name: "Thusia",
            title: "Ancient of Vaults",
            description: "+100% Treasure Chest life when Golden Clicks is activated");
        public static readonly Ancient Mammon = new Ancient(
            id: 8,
            maxLevel: -1,
            name: "Mammon",
            title: "Ancient of Greed",
            description: "+5% Gold");
        public static readonly Ancient Mimzee = new Ancient(
            id: 9,
            maxLevel: -1,
            name: "Mimzee",
            title: "Ancient of Riches",
            description: "+50% treasure chest Gold");
        public static readonly Ancient Pluto = new Ancient(
            id: 10,
            maxLevel: -1,
            name: "Pluto",
            title: "Ancient of Wealth",
            description: "+30% Golden Clicks Gold");
        public static readonly Ancient Dogcog = new Ancient(
            id: 11,
            maxLevel: 25,
            name: "Dogcog",
            title: "Ancient of Thrift",
            description: "-2% hero cost");
        public static readonly Ancient Fortuna = new Ancient(
            id: 12,
            maxLevel: 40,
            name: "Fortuna",
            title: "Ancient of Chance",
            description: "+0.25% chance of 10x Gold");
        public static readonly Ancient Atman = new Ancient(
            id: 13,
            maxLevel: 25,
            name: "Atman",
            title: "Ancient of Souls",
            description: "+1% primal boss chance");
        public static readonly Ancient Dora = new Ancient(
            id: 14,
            maxLevel: 50,
            name: "Dora",
            title: "Ancient of Discovery",
            description: "+20% more Treasure Chests");
        public static readonly Ancient Bhaal = new Ancient(
            id: 15,
            maxLevel: -1,
            name: "Bhaal",
            title: "Ancient of Murder",
            description: "+15% critical damage");
        public static readonly Ancient Morgulis = new Ancient(
            id: 16,
            maxLevel: -1,
            name: "Morgulis",
            title: "Ancient of Death",
            description: "+11% DPS per Hero Soul Spent (cumulative)");
        public static readonly Ancient Chronos = new Ancient(
            id: 17,
            maxLevel: -1,
            name: "Chronos",
            title: "Ancient of Time",
            description: "+5 seconds to Boss Fight timers");
        public static readonly Ancient Bubos = new Ancient(
            id: 18,
            maxLevel: 25,
            name: "Bubos",
            title: "Ancient of Diseases",
            description: "-2% boss life");
        public static readonly Ancient Fragsworth = new Ancient(
            id: 19,
            maxLevel: -1,
            name: "Fragsworth",
            title: "Ancient of Wrath",
            description: "+20% click damage");
        public static readonly Ancient Vaagur = new Ancient(
            id: 20,
            maxLevel: 15,
            name: "Vaagur",
            title: "Ancient of Impatience",
            description: "-5% skill cooldowns");
        public static readonly Ancient Kumawakamaru = new Ancient(
            id: 21,
            maxLevel: 5,
            name: "Kumawakamaru",
            title: "Ancient of Shadows",
            description: "-1 monsters to advance");
        public static readonly Ancient Chawedo = new Ancient(
            id: 22,
            maxLevel: 30,
            name: "Chawedo",
            title: "Ancient of Agitation",
            description: "+2s Clickstorm duration");
        public static readonly Ancient Hecatoncheir = new Ancient(
            id: 23,
            maxLevel: 30,
            name: "Hecatoncheir",
            title: "Ancient of Wallops",
            description: "+2s Super Clicks duration");
        public static readonly Ancient Berserker = new Ancient(
            id: 24,
            maxLevel: 30,
            name: "Berserker",
            title: "Ancient of Rage",
            description: "+2s Powersurge duration");
        public static readonly Ancient Sniperino = new Ancient(
            id: 25,
            maxLevel: 30,
            name: "Sniperino",
            title: "Ancient of Accuracy",
            description: "+2s Lucky Strikes duration");
        public static readonly Ancient Kleptos = new Ancient(
            id: 26,
            maxLevel: 30,
            name: "Kleptos",
            title: "Ancient of Thieves",
            description: "+2s Golden Clicks duration");
        public static readonly Ancient Energon = new Ancient(
            id: 27,
            maxLevel: 30,
            name: "Energon",
            title: "Ancient of Battery Life",
            description: "+2s Metal Detector duration");
        public static readonly Ancient Argaiv = new Ancient(
            id: 28,
            maxLevel: -1,
            name: "Argaiv",
            title: "Ancient of Enhancement",
            description: "+2% Gilded bonus (per Gild)");
        public static readonly Ancient Juggernaut = new Ancient(
            id: 29,
            maxLevel: -1,
            name: "Juggernaut",
            title: "Ancient of Momentum",
            description: "+0.01% DPS per click combo (active clicking)");
        public static readonly Ancient Iris = new Ancient(
            id: 30,
            maxLevel: -1,
            name: "Iris",
            title: "Ancient of Vision",
            description: "+1 to starting zone after Ascension");
        public static readonly Ancient Revloc = new Ancient(
            id: 31,
            maxLevel: 15,
            name: "Revloc",
            title: "Ancient of Luck",
            description: "+1% Chance of double rubies from clickable treasure, when you get a ruby");

        private Ancient(
            int id,
            int maxLevel,
            string name,
            string title,
            string description)
        {
            this.Id = id;
            this.Name = name;
            this.Title = title;
            this.Description = description;

            // Add itself to the static collection
            ancients.Add(this.Id, this);
        }

        public int Id { get; private set; }

        public int MaxLevel { get; private set; }

        public string Name { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public static Ancient Get(int id)
        {
            Ancient ancient;
            return ancients.TryGetValue(id, out ancient) ? ancient : null;
        }
    }
}