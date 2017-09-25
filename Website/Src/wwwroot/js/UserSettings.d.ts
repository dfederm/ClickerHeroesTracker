declare interface IUserSettings
{
    areUploadsPublic: boolean;

    playStyle: string;

    useScientificNotation: boolean;

    scientificNotationThreshold: number;

    useEffectiveLevelForSuggestions: boolean;

    useLogarithmicGraphScale: boolean;

    logarithmicGraphScaleThreshold: number;

    hybridRatio: number;
}

// Serialized singleton for every request
declare var userSettings: IUserSettings;
