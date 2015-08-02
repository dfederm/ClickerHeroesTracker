namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;

    public class Hero
    {
        private static Dictionary<int, Hero> heroes = new Dictionary<int, Hero>();

        // Populated by: http://s3-us-west-2.amazonaws.com/clickerheroes/ancientssoul.html
        // var out = ""; for (var key in Heroes) { var hero = Heroes[key]; out += "public static readonly Hero " + hero.name.replace(/[ ,']/g,"") + " = new Hero(\n    id: " + (parseInt(key)+2) + ",\n    name: \""+ hero.name +"\",\n    cost: " + hero.cost + "d,\n    damage: " + hero.damage + "d);\n" };
        // TODO: move to SQL
        public static readonly Hero CidtheHelpfulAdventurer = new Hero(
            id: 1,
            name: "Cid, the Helpful Adventurer",
            cost: 5d,
            damage: 0d);
        public static readonly Hero TreeBeast = new Hero(
            id: 2,
            name: "Tree Beast",
            cost: 50d,
            damage: 100d);
        public static readonly Hero IvantheDrunkenBrawler = new Hero(
            id: 3,
            name: "Ivan, the Drunken Brawler",
            cost: 250d,
            damage: 440d);
        public static readonly Hero BrittanytheBeachPrincess = new Hero(
            id: 4,
            name: "Brittany, the Beach Princess",
            cost: 1000d,
            damage: 1480d);
        public static readonly Hero TheWanderingFisherman = new Hero(
            id: 5,
            name: "The Wandering Fisherman",
            cost: 4000d,
            damage: 1960d);
        public static readonly Hero BettyClicker = new Hero(
            id: 6,
            name: "Betty Clicker",
            cost: 20000d,
            damage: 976d);
        public static readonly Hero TheMaskedSamurai = new Hero(
            id: 7,
            name: "The Masked Samurai",
            cost: 100000d,
            damage: 74500d);
        public static readonly Hero Leon = new Hero(
            id: 8,
            name: "Leon",
            cost: 400000d,
            damage: 86872d);
        public static readonly Hero TheGreatForestSeer = new Hero(
            id: 9,
            name: "The Great Forest Seer",
            cost: 2500000d,
            damage: 942860d);
        public static readonly Hero AlexatheAssassin = new Hero(
            id: 10,
            name: "Alexa, the Assassin",
            cost: 15000000d,
            damage: 941625d);
        public static readonly Hero NataliaIceApprentice = new Hero(
            id: 11,
            name: "Natalia, Ice Apprentice",
            cost: 100000000d,
            damage: 15640000d);
        public static readonly Hero MercedesDuchessofBlades = new Hero(
            id: 12,
            name: "Mercedes, Duchess of Blades",
            cost: 800000000d,
            damage: 74420000d);
        public static readonly Hero BobbyBountyHunter = new Hero(
            id: 13,
            name: "Bobby, Bounty Hunter",
            cost: 6500000000d,
            damage: 340200000d);
        public static readonly Hero BroyleLindovenFireMage = new Hero(
            id: 14,
            name: "Broyle Lindoven, Fire Mage",
            cost: 50000000000d,
            damage: 694800000d);
        public static readonly Hero SirGeorgeIIKingsGuard = new Hero(
            id: 15,
            name: "Sir George II, King's Guard",
            cost: 450000000000d,
            damage: 9200000000d);
        public static readonly Hero KingMidas = new Hero(
            id: 16,
            name: "King Midas",
            cost: 4000000000000d,
            damage: 3017000000d);
        public static readonly Hero ReferiJeratorIceWizard = new Hero(
            id: 17,
            name: "Referi Jerator, Ice Wizard",
            cost: 36000000000000d,
            damage: 400180000000d);
        public static readonly Hero Abaddon = new Hero(
            id: 18,
            name: "Abaddon",
            cost: 320000000000000d,
            damage: 1492171875000d);
        public static readonly Hero MaZhu = new Hero(
            id: 19,
            name: "Ma Zhu",
            cost: 2700000000000000d,
            damage: 16280000000000d);
        public static readonly Hero Amenhotep = new Hero(
            id: 20,
            name: "Amenhotep",
            cost: 24000000000000000d,
            damage: 10670000000000d);
        public static readonly Hero Beastlord = new Hero(
            id: 21,
            name: "Beastlord",
            cost: 300000000000000000d,
            damage: 393144000000000d);
        public static readonly Hero AthenaGoddessofWar = new Hero(
            id: 22,
            name: "Athena, Goddess of War",
            cost: 9000000000000000000d,
            damage: 17376000000000000d);
        public static readonly Hero AphroditeGoddessofLove = new Hero(
            id: 23,
            name: "Aphrodite, Goddess of Love",
            cost: 350000000000000000000d,
            damage: 497984000000000000d);
        public static readonly Hero ShinatobeWindDeity = new Hero(
            id: 24,
            name: "Shinatobe, Wind Deity",
            cost: 1.4e+22d,
            damage: 7336000000000000000d);
        public static readonly Hero GranttheGeneral = new Hero(
            id: 25,
            name: "Grant, the General",
            cost: 4.199e+24d,
            damage: 808000000000000000000d);
        public static readonly Hero Frostleaf = new Hero(
            id: 26,
            name: "Frostleaf",
            cost: 2.1e+27d,
            damage: 2.98792e+23d);
        public static readonly Hero DreadKnight = new Hero(
            id: 27,
            name: "Dread Knight",
            cost: 1e+40d,
            damage: 2.92e+33d);
        public static readonly Hero Atlas = new Hero(
            id: 28,
            name: "Atlas",
            cost: 1e+55d,
            damage: 2.1500000000000003e+46d);
        public static readonly Hero Terra = new Hero(
            id: 29,
            name: "Terra",
            cost: 1e+70d,
            damage: 1.5852e+59d);
        public static readonly Hero Phthalo = new Hero(
            id: 30,
            name: "Phthalo",
            cost: 1e+85d,
            damage: 1.1678e+72d);
        public static readonly Hero OrntchyaGladeyeDidensyBanana = new Hero(
            id: 31,
            name: "Orntchya Gladeye, Didensy Banana",
            cost: 1e+100d,
            damage: 6.604e+84d);
        public static readonly Hero Lilin = new Hero(
            id: 32,
            name: "Lilin",
            cost: 1e+115d,
            damage: 6.34e+97d);
        public static readonly Hero Cadmia = new Hero(
            id: 33,
            name: "Cadmia",
            cost: 1e+130d,
            damage: 4.67e+110d);
        public static readonly Hero Alabaster = new Hero(
            id: 34,
            name: "Alabaster",
            cost: 1e+145d,
            damage: 3.442e+123d);
        public static readonly Hero Astraea = new Hero(
            id: 35,
            name: "Astraea",
            cost: 1e+160d,
            damage: 2.536e+136d);

        private Hero(
            int id,
            string name,
            double cost,
            double damage)
        {
            this.Id = id;
            this.Name = name;
            this.Cost = cost;
            this.Damage = damage;

            // Add itself to the static collection
            heroes.Add(this.Id, this);
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public double Cost { get; private set; }

        public double Damage { get; private set; }

        public static Hero Get(int id)
        {
            Hero hero;
            return heroes.TryGetValue(id, out hero) ? hero : null;
        }
    }
}