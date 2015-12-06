// <copyright file="Hero.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;

    public class Hero
    {
        private static Dictionary<int, Hero> heroes = new Dictionary<int, Hero>();

        // Cid is populated manually
        public static readonly Hero CidtheHelpfulAdventurer = new Hero(
            id: 1,
            name: "Cid, the Helpful Adventurer",
            cost: 5d,
            upgradeCosts: new[] { 100d, 250d, 1e3d, 8e3d, 80e3d, 400e3d, 4e6d },
            damage: 0d);

        // Populated by: http://s3-us-west-2.amazonaws.com/clickerheroes/ancientssoul.html
        // var out = ""; for (var key in Heroes) { var hero = Heroes[key]; out += "public static readonly Hero " + hero.name.replace(/[ ,']/g,"") + " = new Hero(\n    id: " + (parseInt(key)+2) + ",\n    name: \""+ hero.name +"\",\n    cost: " + hero.cost + "d,\n    damage: " + hero.damage + "d,\n    upgradeCosts: new[] { " + hero.upgrades.toString().replace(/,/g, 'd, ') + "d }" + (hero.has5x ? ",\n    isRanger: true" : "") + ");\n" };
        // TODO: move to SQL
        public static readonly Hero TreeBeast = new Hero(
            id: 2,
            name: "Tree Beast",
            cost: 50d,
            damage: 100d,
            upgradeCosts: new[] { 500d, 1250d, 5000d, 40000d, 400000d });

        public static readonly Hero IvantheDrunkenBrawler = new Hero(
            id: 3,
            name: "Ivan, the Drunken Brawler",
            cost: 250d,
            damage: 440d,
            upgradeCosts: new[] { 2500d, 6250d, 25000d, 200000d, 2000000d, 10000000d });

        public static readonly Hero BrittanytheBeachPrincess = new Hero(
            id: 4,
            name: "Brittany, the Beach Princess",
            cost: 1000d,
            damage: 1480d,
            upgradeCosts: new[] { 10000d, 25000d, 100000d, 800000d });

        public static readonly Hero TheWanderingFisherman = new Hero(
            id: 5,
            name: "The Wandering Fisherman",
            cost: 4000d,
            damage: 1960d,
            upgradeCosts: new[] { 40000d, 100000d, 400000d, 3200000d, 32000000d });

        public static readonly Hero BettyClicker = new Hero(
            id: 6,
            name: "Betty Clicker",
            cost: 20000d,
            damage: 976d,
            upgradeCosts: new[] { 200000d, 500000d, 2000000d, 16000000d, 160000000d });

        public static readonly Hero TheMaskedSamurai = new Hero(
            id: 7,
            name: "The Masked Samurai",
            cost: 100000d,
            damage: 74500d,
            upgradeCosts: new[] { 1000000d, 2500000d, 10000000d, 80000000d });

        public static readonly Hero Leon = new Hero(
            id: 8,
            name: "Leon",
            cost: 400000d,
            damage: 86872d,
            upgradeCosts: new[] { 4000000d, 10000000d, 40000000d, 320000000d });

        public static readonly Hero TheGreatForestSeer = new Hero(
            id: 9,
            name: "The Great Forest Seer",
            cost: 2500000d,
            damage: 942860d,
            upgradeCosts: new[] { 25000000d, 62500000d, 250000000d, 2000000000d });

        public static readonly Hero AlexatheAssassin = new Hero(
            id: 10,
            name: "Alexa, the Assassin",
            cost: 15000000d,
            damage: 941625d,
            upgradeCosts: new[] { 150000000d, 375000000d, 1500000000d, 12000000000d, 120000000000d });

        public static readonly Hero NataliaIceApprentice = new Hero(
            id: 11,
            name: "Natalia, Ice Apprentice",
            cost: 100000000d,
            damage: 15640000d,
            upgradeCosts: new[] { 1000000000d, 2500000000d, 10000000000d, 80000000000d });

        public static readonly Hero MercedesDuchessofBlades = new Hero(
            id: 12,
            name: "Mercedes, Duchess of Blades",
            cost: 800000000d,
            damage: 74420000d,
            upgradeCosts: new[] { 8000000000d, 20000000000d, 80000000000d, 640000000000d, 6400000000000d });

        public static readonly Hero BobbyBountyHunter = new Hero(
            id: 13,
            name: "Bobby, Bounty Hunter",
            cost: 6500000000d,
            damage: 340200000d,
            upgradeCosts: new[] { 65000000000d, 162000000000d, 650000000000d, 5200000000000d, 52000000000000d });

        public static readonly Hero BroyleLindovenFireMage = new Hero(
            id: 14,
            name: "Broyle Lindoven, Fire Mage",
            cost: 50000000000d,
            damage: 694800000d,
            upgradeCosts: new[] { 500000000000d, 1250000000000d, 5000000000000d, 40000000000000d, 400000000000000d });

        public static readonly Hero SirGeorgeIIKingsGuard = new Hero(
            id: 15,
            name: "Sir George II, King's Guard",
            cost: 450000000000d,
            damage: 9200000000d,
            upgradeCosts: new[] { 4500000000000d, 11250000000000d, 45000000000000d, 360000000000000d, 3600000000000000d });

        public static readonly Hero KingMidas = new Hero(
            id: 16,
            name: "King Midas",
            cost: 4000000000000d,
            damage: 3017000000d,
            upgradeCosts: new[] { 40000000000000d, 100000000000000d, 400000000000000d, 3200000000000000d, 32000000000000000d, 160000000000000000d });

        public static readonly Hero ReferiJeratorIceWizard = new Hero(
            id: 17,
            name: "Referi Jerator, Ice Wizard",
            cost: 36000000000000d,
            damage: 400180000000d,
            upgradeCosts: new[] { 360000000000000d, 900000000000000d, 3600000000000000d, 28800000000000000d, 288000000000000000d });

        public static readonly Hero Abaddon = new Hero(
            id: 18,
            name: "Abaddon",
            cost: 320000000000000d,
            damage: 1492171875000d,
            upgradeCosts: new[] { 3200000000000000d, 8000000000000000d, 32000000000000000d, 256000000000000000d });

        public static readonly Hero MaZhu = new Hero(
            id: 19,
            name: "Ma Zhu",
            cost: 2700000000000000d,
            damage: 16280000000000d,
            upgradeCosts: new[] { 27000000000000000d, 67500000000000000d, 270000000000000000d, 2160000000000000000d });

        public static readonly Hero Amenhotep = new Hero(
            id: 20,
            name: "Amenhotep",
            cost: 24000000000000000d,
            damage: 10670000000000d,
            upgradeCosts: new[] { 240000000000000000d, 600000000000000000d, 2400000000000000000d });

        public static readonly Hero Beastlord = new Hero(
            id: 21,
            name: "Beastlord",
            cost: 300000000000000000d,
            damage: 393144000000000d,
            upgradeCosts: new[] { 3000000000000000000d, 7500000000000000000d, 30000000000000000000d, 240000000000000000000d, 2.4e+21d });

        public static readonly Hero AthenaGoddessofWar = new Hero(
            id: 22,
            name: "Athena, Goddess of War",
            cost: 9000000000000000000d,
            damage: 17376000000000000d,
            upgradeCosts: new[] { 90000000000000000000d, 225000000000000000000d, 900000000000000000000d, 0d, 7.2e+21d });

        public static readonly Hero AphroditeGoddessofLove = new Hero(
            id: 23,
            name: "Aphrodite, Goddess of Love",
            cost: 350000000000000000000d,
            damage: 497984000000000000d,
            upgradeCosts: new[] { 3.5e+21d, 8.75e+21d, 3.5e+22d, 0d, 2.8e+23d, 2.8e+24d });

        public static readonly Hero ShinatobeWindDeity = new Hero(
            id: 24,
            name: "Shinatobe, Wind Deity",
            cost: 1.4e+22d,
            damage: 7336000000000000000d,
            upgradeCosts: new[] { 1.4e+23d, 3.5e+23d, 1.4e+24d, 1.12e+25d, 1.12e+26d });

        public static readonly Hero GranttheGeneral = new Hero(
            id: 25,
            name: "Grant, the General",
            cost: 4.199e+24d,
            damage: 808000000000000000000d,
            upgradeCosts: new[] { 4.1999e+25d, 1.04e+26d, 4.19e+26d, 3.359e+27d });

        public static readonly Hero Frostleaf = new Hero(
            id: 26,
            name: "Frostleaf",
            cost: 2.1e+27d,
            damage: 2.98792e+23d,
            upgradeCosts: new[] { 2.1e+28d, 5.2499e+28d, 2.09e+29d, 1.679e+30d });

        public static readonly Hero DreadKnight = new Hero(
            id: 27,
            name: "Dread Knight",
            cost: 1e+40d,
            damage: 2.92e+33d,
            upgradeCosts: new[] { 1e+41d, 2.5e+41d, 1e+42d, 0d, 8e+42d },
            isRanger: true);

        public static readonly Hero Atlas = new Hero(
            id: 28,
            name: "Atlas",
            cost: 1e+55d,
            damage: 2.1500000000000003e+46d,
            upgradeCosts: new[] { 1e+56d, 2.5e+56d, 1e+57d, 0d, 8e+57d },
            isRanger: true);

        public static readonly Hero Terra = new Hero(
            id: 29,
            name: "Terra",
            cost: 1e+70d,
            damage: 1.5852e+59d,
            upgradeCosts: new[] { 1e+71d, 2.5e+71d, 1e+72d, 0d, 8e+72d },
            isRanger: true);

        public static readonly Hero Phthalo = new Hero(
            id: 30,
            name: "Phthalo",
            cost: 1e+85d,
            damage: 1.1678e+72d,
            upgradeCosts: new[] { 1e+86d, 2.5e+86d, 1e+87d, 0d, 8e+87d },
            isRanger: true);

        public static readonly Hero OrntchyaGladeyeDidensyBanana = new Hero(
            id: 31,
            name: "Orntchya Gladeye, Didensy Banana",
            cost: 1e+100d,
            damage: 6.604e+84d,
            upgradeCosts: new[] { 1e+101d, 2.5e+101d, 1e+102d, 0d, 8e+102d },
            isRanger: true);

        public static readonly Hero Lilin = new Hero(
            id: 32,
            name: "Lilin",
            cost: 1e+115d,
            damage: 6.34e+97d,
            upgradeCosts: new[] { 1e+116d, 2.5e+116d, 1e+117d, 0d, 8e+117d },
            isRanger: true);

        public static readonly Hero Cadmia = new Hero(
            id: 33,
            name: "Cadmia",
            cost: 1e+130d,
            damage: 4.67e+110d,
            upgradeCosts: new[] { 1e+131d, 2.5e+131d, 1e+132d, 0d, 8e+132d },
            isRanger: true);

        public static readonly Hero Alabaster = new Hero(
            id: 34,
            name: "Alabaster",
            cost: 1e+145d,
            damage: 3.442e+123d,
            upgradeCosts: new[] { 1e+146d, 2.5e+146d, 1e+147d, 0d, 8e+147d },
            isRanger: true);

        public static readonly Hero Astraea = new Hero(
            id: 35,
            name: "Astraea",
            cost: 1e+160d,
            damage: 2.536e+136d,
            upgradeCosts: new[] { 1e+161d, 2.5e+161d, 1e+162d, 0d, 8e+162d },
            isRanger: true);

        private Hero(
            int id,
            string name,
            double cost,
            double damage,
            double[] upgradeCosts,
            bool isRanger = false)
        {
            this.Id = id;
            this.Name = name;
            this.Cost = cost;
            this.Damage = damage;
            this.UpgradeCosts = upgradeCosts;
            this.IsRanger = isRanger;

            // Add itself to the static collection
            heroes.Add(this.Id, this);
        }

        public static IEnumerable<Hero> All
        {
            get
            {
                return heroes.Values;
            }
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public double Cost { get; private set; }

        public double Damage { get; private set; }

        public double[] UpgradeCosts { get; private set; }

        public bool IsRanger { get; private set; }

        public static Hero Get(int id)
        {
            Hero hero;
            return heroes.TryGetValue(id, out hero) ? hero : null;
        }
    }
}