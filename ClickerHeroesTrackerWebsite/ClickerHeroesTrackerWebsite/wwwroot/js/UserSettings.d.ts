declare interface IUserSettings
{
    areUploadsPublic: boolean;

    useReducedSolomonFormula: boolean;

    playStyle: string;

    useExperimentalStats: boolean;

    useScientificNotation: boolean;

    scientificNotationThreshold: number;
}

// Serialized singleton for every request
declare var userSettings: IUserSettings;
