namespace ClickerHeroesTrackerWebsite.Models.Game
{
    using System.Collections.Generic;

    public class HeroUpgrade
    {
        private static Dictionary<int, HeroUpgrade> upgrades = new Dictionary<int, HeroUpgrade>();

        private static double maximumDamageMultiplier = 1d;

        static HeroUpgrade()
        {
            // Populated by: http://s3-us-west-2.amazonaws.com/clickerheroes/ancientssoul.html
            // var out = ""; for (var key in Upgrades) { var damageMultiplier = Upgrades[key]; out += "new HeroUpgrade(\n    id: " + key + ",\n    damageMultiplier: 1."+ damageMultiplier +"d);\n" };
            // Lots of missing upgrades!
            // TODO: move to SQL
            new HeroUpgrade(
                id: 12,
                damageMultiplier: 1.25d);
            new HeroUpgrade(
                id: 25,
                damageMultiplier: 1.20d);
            new HeroUpgrade(
                id: 26,
                damageMultiplier: 1.20d);
            new HeroUpgrade(
                id: 27,
                damageMultiplier: 1.20d);
            new HeroUpgrade(
                id: 28,
                damageMultiplier: 1.20d);
            new HeroUpgrade(
                id: 36,
                damageMultiplier: 1.25d);
            new HeroUpgrade(
                id: 57,
                damageMultiplier: 1.25d);
            new HeroUpgrade(
                id: 82,
                damageMultiplier: 1.20d);
            new HeroUpgrade(
                id: 83,
                damageMultiplier: 1.20d);
            new HeroUpgrade(
                id: 87,
                damageMultiplier: 1.10d);
            new HeroUpgrade(
                id: 97,
                damageMultiplier: 1.10d);
            new HeroUpgrade(
                id: 120,
                damageMultiplier: 1.25d);
            new HeroUpgrade(
                id: 122,
                damageMultiplier: 1.25d);
            new HeroUpgrade(
                id: 126,
                damageMultiplier: 1.25d);
        }

        private HeroUpgrade(
            int id,
            double damageMultiplier)
        {
            this.Id = id;
            this.DamageMultiplier = damageMultiplier;

            // Add itself to the static collection
            upgrades.Add(this.Id, this);

            // Add to the maximum possible damage multiplier, ie. if one were to have them all.
            maximumDamageMultiplier *= damageMultiplier;
        }

        public static double MaximumDamageMultiplier
        {
            get
            {
                return maximumDamageMultiplier;
            }
        } 

        public int Id { get; private set; }

        public double DamageMultiplier { get; private set; }

        public static HeroUpgrade Get(int id)
        {
            HeroUpgrade upgrade;
            return upgrades.TryGetValue(id, out upgrade) ? upgrade : null;
        }
    }
}