import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: "exponential" })
export class ExponentialPipe implements PipeTransform
{
    // TODO get the user's real settings
    private userSettings =
    {
        areUploadsPublic: true,
        hybridRatio: 1,
        logarithmicGraphScaleThreshold: 1000000,
        playStyle: "hybrid",
        scientificNotationThreshold: 100000,
        useEffectiveLevelForSuggestions: false,
        useExperimentalStats: true,
        useLogarithmicGraphScale: true,
        useReducedSolomonFormula: false,
        useScientificNotation: true,
    };

    public transform(value: number): string
    {
        if (!value)
        {
            value = 0;
        }

        const useScientificNotation = this.userSettings.useScientificNotation && Math.abs(value) > this.userSettings.scientificNotationThreshold;
        return useScientificNotation
            ? value.toExponential(3)
            : value.toLocaleString();
    }
}
