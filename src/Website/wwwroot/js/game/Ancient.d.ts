declare interface IAncient
{
    id: number;

    name: string;

    nonTranscendent: boolean;

    levelCostFormula: string;

    shortName: string;
}

// Serialized singleton
declare var ancientsData: IMap<IAncient>;
