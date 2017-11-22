import { Hero } from "./hero";

export class HeroCollection {
    constructor(
        public heroes: { [id: number]: Hero },
    ) { }

    public getById(id: number): Hero {
        return this.heroes[id];
    }

    public hasHeroWithLevel(param1: number, param2: number): boolean {
        let hero = this.getById(param1);
        return hero && hero.level >= param2;
    }

    public getTotalEpicLevels(): number {
        let epicLevels = 0;
        for (let heroId in this.heroes) {
            let hero = this.heroes[heroId];
            epicLevels += hero.epicLevel;
        }

        return epicLevels;
    }

    public addEpicLevel(heroId: number, numLevels: number = 1): void {
        let hero = this.getById(heroId);
        hero.epicLevel = hero.epicLevel + numLevels;
    }

    public addGildLevels(gildLevels: number[]): void {
        let heroId = 1;
        while (heroId < gildLevels.length) {
            this.addEpicLevel(heroId, gildLevels[heroId]);
            heroId++;
        }
    }
}
