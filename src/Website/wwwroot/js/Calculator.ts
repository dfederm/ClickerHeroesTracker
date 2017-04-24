namespace Calculator
{
    "use strict";

    const exponentialRegex = new RegExp("^(\\d+(\\.\\d+)?)e\\+?(\\d+)$", "i");

    // An index for quick lookup of ancient cost formulas.
    // Each formula gets the sum of the cost of the ancient from 1 to N.
    const ancientCostFormulas = getAncientCostFormulas();

    const optimalOutsiderLevels = getOptimalOutsiderLevels();

    let lastUpload: IUpload;

    function handleSuccess(upload: IUpload): void
    {
        lastUpload = upload;

        const userNameElements = Helpers.getElementsByDataType("uploadUserName");
        for (let i = 0; i < userNameElements.length; i++)
        {
            if (upload.user && upload.user.name)
            {
                userNameElements[i].textContent = upload.user.name;
            }
            else
            {
                userNameElements[i].classList.add("text-muted");
                userNameElements[i].textContent = "(Anonymous)";
            }
        }

        const submitTimeElements = Helpers.getElementsByDataType("uploadSubmitTime");
        if (upload.timeSubmitted)
        {
            const timeSubmitted = new Date(upload.timeSubmitted);
            for (let i = 0; i < submitTimeElements.length; i++)
            {
                submitTimeElements[i].textContent = timeSubmitted.toLocaleString();
            }
        }

        const playStyleElements = Helpers.getElementsByDataType("uploadPlayStyle");
        if (upload.playStyle)
        {
            for (let i = 0; i < playStyleElements.length; i++)
            {
                playStyleElements[i].textContent = upload.playStyle.toTitleCase();
            }
        }

        const uploadContentElements = Helpers.getElementsByDataType("uploadContent");
        if (upload.uploadContent)
        {
            for (let i = 0; i < uploadContentElements.length; i++)
            {
                uploadContentElements[i].textContent = upload.uploadContent;
            }
        }

        if (upload.stats)
        {
            for (let statType in upload.stats)
            {
                hydrateStat(upload.stats, statType, upload.stats[statType]);
            }

            hydrateAncientSuggestions();

            calculateOutsiderSuggestions();

            if (upload.stats["transcendentPower"] === 0)
            {
                const solomonTooltipElements = Helpers.getElementsByDataType("solomonTooltip");
                for (let i = 0; i < solomonTooltipElements.length; i++)
                {
                    solomonTooltipElements[i].classList.remove("hidden");
                }
            }
        }
    }

    function hydrateStat(stats: IMap<number>, statType: string, statValue: number): void
    {
        const statElements = Helpers.getElementsByDataType(statType);
        if (statElements)
        {
            const useScientificNotation = userSettings.useScientificNotation && Math.abs(statValue) > userSettings.scientificNotationThreshold;

            let fullText = statValue.toLocaleString();
            let displayText = useScientificNotation ? statValue.toExponential(3) : fullText;

            if (statType.indexOf("ancient") === 0)
            {
                const itemStatType = statType.replace("ancient", "item");
                const tooltipType = statType + "Tooltip";

                const itemStatValue = stats[itemStatType] || 0;

                if (itemStatValue > 0)
                {
                    const tooltipElements = Helpers.getElementsByDataType(tooltipType);
                    if (itemStatValue > 0)
                    {
                        const useScientificNotationItem = userSettings.useScientificNotation && Math.abs(itemStatValue) > userSettings.scientificNotationThreshold;
                        const itemDisplayText = useScientificNotationItem
                            ? itemStatValue.toExponential(3)
                            : itemStatValue.toLocaleString();

                        const effectiveLevelValue = Math.floor(statValue + itemStatValue);
                        const useScientificNotationEffectiveLevel = userSettings.useScientificNotation && Math.abs(effectiveLevelValue) > userSettings.scientificNotationThreshold;
                        const effectiveLevelDisplayText = useScientificNotationEffectiveLevel
                            ? effectiveLevelValue.toExponential(3)
                            : effectiveLevelValue.toLocaleString();

                        const ancientLevelElement = document.createElement("div");
                        ancientLevelElement.appendChild(document.createTextNode("Ancient Level: "));
                        ancientLevelElement.appendChild(document.createTextNode(displayText));

                        const itemLevelElement = document.createElement("div");
                        itemLevelElement.appendChild(document.createTextNode("Relic Level: "));
                        itemLevelElement.appendChild(document.createTextNode(itemDisplayText));

                        const effectiveLevelElement = document.createElement("div");
                        effectiveLevelElement.appendChild(document.createTextNode("Effective Level: "));
                        effectiveLevelElement.appendChild(document.createTextNode(effectiveLevelDisplayText));

                        if (userSettings.useEffectiveLevelForSuggestions)
                        {
                            displayText = effectiveLevelDisplayText;
                        }

                        for (let i = 0; i < tooltipElements.length; i++)
                        {
                            const tooltipElement = tooltipElements[i];
                            tooltipElement.setAttribute("data-original-title", ancientLevelElement.outerHTML + itemLevelElement.outerHTML + effectiveLevelElement.outerHTML);
                            tooltipElement.classList.remove("hidden");
                        }
                    }
                }
            }

            if (statType.indexOf("diff") === 0)
            {
                for (let i = 0; i < statElements.length; i++)
                {
                    const statElement = statElements[i];
                    $(statElement).tooltip({
                        placement: "right",
                        title: "Click to copy to clipboard",
                    });
                    statElement.classList.add("clickable");
                    statElement.addEventListener("click", function (): void
                    {
                        let copyValue = formatForClipboard(statValue);
                        Helpers.copyToClipboard(copyValue);
                    });
                }
            }

            if (statType.indexOf("transcendentPower") === 0)
            {
                displayText = (statValue * 100).toFixed(2) + "%";
            }

            for (let i = 0; i < statElements.length; i++)
            {
                if (useScientificNotation)
                {
                    statElements[i].title = fullText;
                }

                statElements[i].textContent = displayText;
            }
        }
    }

    function formatForClipboard(num: number): string
    {
        // The game can't handle pasting in decimal points, so we'll just use an altered sci-not form that excludes the decimal (eg. 1.234e5 => 1234e2)
        if (num >= 1e6)
        {
            let str = num.toExponential();
            let groups = exponentialRegex.exec(str);
            let n = parseFloat(groups[1]);
            let exponent = parseInt(groups[3]);

            n *= 1e5;
            n = Math.floor(n);
            exponent -= 5;

            return exponent === 0
                ? n.toFixed()
                : (n.toFixed() + "e" + exponent);
        }
        else
        {
            return num.toFixed(0);
        }
    }

    function hydrateAncientSuggestion(stats: IMap<number>, ancient: string, suggestedLevel: number): void
    {
        hydrateStat(stats, "suggested" + ancient, suggestedLevel);
        hydrateStat(stats, "diff" + ancient, suggestedLevel - getCurrentAncientLevel(stats, ancient));
    }

    function displayFailure(): void
    {
        // BUGBUG 51: Create Loading and Failure states for ajax loading
    }

    function hydrateAncientSuggestions(): void
    {
        if (!lastUpload)
        {
            return;
        }

        const stats = lastUpload.stats;
        if (!stats)
        {
            return;
        }

        const availableSoulsSuggestionsLatency = "AncientSuggestions";
        appInsights.startTrackEvent(availableSoulsSuggestionsLatency);

        const primaryAncient = lastUpload.playStyle === "active"
            ? "Fragsworth"
            : "Siyalatas";

        let suggestedLevels: IMap<number>;

        const suggestionType = $("input[name='SuggestionType']:checked").val() as string;
        const useSoulsFromAscensionElement = document.getElementById("UseSoulsFromAscension") as HTMLInputElement;
        const useSoulsFromAscensionContainer = useSoulsFromAscensionElement.parentElement.parentElement;
        if (suggestionType === "AvailableSouls")
        {
            useSoulsFromAscensionContainer.classList.remove("hidden");

            let availableSouls = stats["heroSouls"] || 0;
            if (useSoulsFromAscensionElement.checked)
            {
                availableSouls += stats["pendingSouls"] || 0;
            }

            /*
                As an optimization, instead of incrementing only by 1 each time,
                we increment by increasing by a power of 2 until we can no longer afford it.
                Then we back off by a power of 2 until we're back to trying just 1. In this
                way we only calculate the suggestions log(n) times instead of n times where n
                is the optimial primary ancient level.
            */
            let power = 0;
            let primaryAncientLevel = 0;
            let powerChange = 1;

            // Seed with some values in case nothing can be afforded
            suggestedLevels = calculateAncientSuggestions(primaryAncientLevel);

            while (power >= 0)
            {
                // If we're on our way up we're trying to find the limit, so use the power.
                let newPrimaryAncientLevel = Math.pow(2, power);
                if (powerChange < 0)
                {
                    // If we're on the way down we're filling the space, so add the last affordable value.
                    newPrimaryAncientLevel += primaryAncientLevel;
                }

                const newSuggestedLevels = calculateAncientSuggestions(newPrimaryAncientLevel);
                if (availableSouls >= getTotalAncientCost(newSuggestedLevels, stats))
                {
                    primaryAncientLevel = newPrimaryAncientLevel;
                    suggestedLevels = newSuggestedLevels;
                }
                else if (powerChange > 0)
                {
                    // We found the limit, so reverse the direction to try and fill.
                    powerChange = -1;
                }

                power += powerChange;
            }

            // Ensure we don't suggest removing levels
            for (let ancient in suggestedLevels)
            {
                suggestedLevels[ancient] = Math.max(suggestedLevels[ancient], getCurrentAncientLevel(stats, ancient));
            }
        }
        else
        {
            useSoulsFromAscensionContainer.classList.add("hidden");
            suggestedLevels = calculateAncientSuggestions();

            // Special-case the primary ancient
            const suggestedPrimaryAncientElements = Helpers.getElementsByDataType("suggested" + primaryAncient);
            for (let i = 0; i < suggestedPrimaryAncientElements.length; i++)
            {
                const suggestedPrimaryAncientElement = suggestedPrimaryAncientElements[i];
                suggestedPrimaryAncientElement.textContent = "N/A ";

                const suggestedPrimaryAncientTooltip = document.createElement("span");
                suggestedPrimaryAncientTooltip.classList.add("text-muted");
                suggestedPrimaryAncientTooltip.setAttribute("data-toggle", "tooltip");
                suggestedPrimaryAncientTooltip.setAttribute("data-placement", "bottom");
                suggestedPrimaryAncientTooltip.title = "The formulae are based on this ancient. If all suggestions below are negative or zero, level this ancient.";
                suggestedPrimaryAncientTooltip.textContent = "(?)";

                suggestedPrimaryAncientElement.appendChild(suggestedPrimaryAncientTooltip);

                // Wire up tooltip
                $(suggestedPrimaryAncientTooltip).tooltip();
            }

            const diffPrimaryAncientElements = Helpers.getElementsByDataType("diff" + primaryAncient);
            for (let i = 0; i < diffPrimaryAncientElements.length; i++)
            {
                diffPrimaryAncientElements[i].textContent = "";
            }
        }

        for (let ancient in suggestedLevels)
        {
            hydrateAncientSuggestion(stats, ancient, suggestedLevels[ancient]);
        }

        appInsights.stopTrackEvent(
            availableSoulsSuggestionsLatency,
            {
                suggestionType: suggestionType,
                useSoulsFromAscension: useSoulsFromAscensionElement.checked.toString(),
            });
    }

    function calculateAncientSuggestions(currentPrimaryAncientLevel?: number): IMap<number>
    {
        const stats = lastUpload.stats;
        const playStyle = lastUpload.playStyle;

        const suggestedLevels: IMap<number> = {};

        const primaryAncient = playStyle === "active" ? "Fragsworth" : "Siyalatas";
        if (currentPrimaryAncientLevel === undefined)
        {
            // Use the current level, but don't use it in the suggestions.
            currentPrimaryAncientLevel = getCurrentAncientLevel(stats, primaryAncient);
        }
        else
        {
            // When provided, add it to the suggestions
            suggestedLevels[primaryAncient] = currentPrimaryAncientLevel;
        }

        const currentBubosLevel = getCurrentAncientLevel(stats, "Bubos");
        const currentChronosLevel = getCurrentAncientLevel(stats, "Chronos");
        const currentDoraLevel = getCurrentAncientLevel(stats, "Dora");
        const currentDogcogLevel = getCurrentAncientLevel(stats, "Dogcog");
        const currentFortunaLevel = getCurrentAncientLevel(stats, "Fortuna");
        const currentAtmanLevel = getCurrentAncientLevel(stats, "Atman");
        const currentKumaLevel = getCurrentAncientLevel(stats, "Kumawakamaru");

        const highestZone = stats["highestZoneThisTranscension"];
        const transcendentPower = stats["transcendentPower"];

        const lnPrimary = Math.log(currentPrimaryAncientLevel);
        const hpScale = 1.145 + (0.005 * Math.floor(highestZone / 500));
        const alpha = transcendentPower === 0 ? 0 : 1.4067 * Math.log(1 + transcendentPower) / Math.log(hpScale);
        const lnAlpha = transcendentPower === 0 ? 0 : Math.log(alpha);

        // Common formulas across play styles
        suggestedLevels["Argaiv"] = currentPrimaryAncientLevel;
        suggestedLevels["Atman"] = (2.832 * lnPrimary) - (1.416 * lnAlpha) - (1.416 * Math.log((4 / 3) - Math.pow(Math.E, -0.013 * currentAtmanLevel))) - 6.613;
        suggestedLevels["Bubos"] = (2.8 * lnPrimary) - (1.4 * Math.log(1 + Math.pow(Math.E, -0.02 * currentBubosLevel))) - 5.94;
        suggestedLevels["Chronos"] = (2.75 * lnPrimary) - (1.375 * Math.log(2 - Math.pow(Math.E, -0.034 * currentChronosLevel))) - 5.1;
        suggestedLevels["Dogcog"] = (2.844 * lnPrimary) - (1.422 * Math.log((1 / 99) + Math.pow(Math.E, -0.01 * currentDogcogLevel))) - 7.232;
        suggestedLevels["Dora"] = (2.877 * lnPrimary) - (1.4365 * Math.log((100 / 99) - Math.pow(Math.E, -0.002 * currentDoraLevel))) - 9.63;
        suggestedLevels["Fortuna"] = (2.875 * lnPrimary) - (1.4375 * Math.log((10 / 9) - Math.pow(Math.E, -0.0025 * currentFortunaLevel))) - 9.3;
        suggestedLevels["Kumawakamaru"] = (2.844 * lnPrimary) - (1.422 * lnAlpha) - (1.422 * Math.log(0.25 + Math.pow(Math.E, -0.001 * currentKumaLevel))) - 7.014;
        suggestedLevels["Mammon"] = suggestedLevels["Mimzee"] = currentPrimaryAncientLevel * 0.926;
        suggestedLevels["Morgulis"] = currentPrimaryAncientLevel * currentPrimaryAncientLevel;
        suggestedLevels["Solomon"] = stats["transcendentPower"] > 0
            ? Math.pow(currentPrimaryAncientLevel, 0.8) / Math.pow(alpha, 0.4)
            : getPreTranscendentSuggestedSolomonLevel(currentPrimaryAncientLevel, playStyle);

        // Math per play style
        switch (playStyle)
        {
            case "idle":
                suggestedLevels["Libertas"] = suggestedLevels["Mammon"];
                suggestedLevels["Nogardnit"] = Math.pow(suggestedLevels["Libertas"], 0.8);
                break;
            case "hybrid":
                const hybridRatioReciprocal = 1 / userSettings.hybridRatio;
                suggestedLevels["Bhaal"] = suggestedLevels["Fragsworth"] = hybridRatioReciprocal * currentPrimaryAncientLevel;
                suggestedLevels["Juggernaut"] = Math.pow(suggestedLevels["Fragsworth"], 0.8);
                suggestedLevels["Libertas"] = suggestedLevels["Mammon"];
                suggestedLevels["Nogardnit"] = Math.pow(suggestedLevels["Libertas"], 0.8);
                break;
            case "active":
                suggestedLevels["Bhaal"] = currentPrimaryAncientLevel;
                suggestedLevels["Juggernaut"] = Math.pow(currentPrimaryAncientLevel, 0.8);
                break;
        }

        // Normalize the values
        for (let ancient in suggestedLevels)
        {
            suggestedLevels[ancient] = Math.max(Math.round(suggestedLevels[ancient]), 0);
        }

        return suggestedLevels;
    }

    function calculateOutsiderSuggestions(): void
    {
        const stats = lastUpload.stats;
        if (!stats)
        {
            return;
        }

        let ancientSouls = stats["totalAncientSouls"];
        if (ancientSouls === 0)
        {
            return;
        }

        let showLowAncientSoulWarning = false;
        let showMissingSimulationWarning = false;

        let suggestedXyl = 0;
        let suggestedChor = 0;
        let suggestedPhan = 0;
        let suggestedBorb = 0;
        let suggestedPony = 0;

        // Less ancient souls than the simulation data supported. We can try to guess though.
        // Our guess just alternates leveling Xyl and Pony until Xylk hits 7 and then dump into Pony unti lit matches the 30 AS simulation data.
        if (ancientSouls < 30)
        {
            showLowAncientSoulWarning = true;
            if (ancientSouls < 14)
            {
                suggestedXyl = Math.ceil(ancientSouls / 2);
                suggestedPony = Math.floor(ancientSouls / 2);
            }
            else
            {
                suggestedXyl = 7;
                suggestedPony = ancientSouls - 7;
            }
        }
        else
        {
            if (ancientSouls > 210)
            {
                showMissingSimulationWarning = true;
                if (ancientSouls >= 1500)
                {
                    ancientSouls = 1500;
                }
                else if (ancientSouls >= 500)
                {
                    ancientSouls -= ancientSouls % 100;
                }
                else
                {
                    ancientSouls -= ancientSouls % 10;
                }
            }

            const outsiderLevels = optimalOutsiderLevels[ancientSouls];
            if (outsiderLevels === null)
            {
                // Should not happen.
                throw Error("Could not look up optimal outsider levels for " + ancientSouls + " ancient souls. Raw ancient souls: " + Helpers.getElementsByDataType("totalAncientSouls")[0].textContent);
            }

            suggestedXyl = outsiderLevels[0];
            suggestedChor = outsiderLevels[1];
            suggestedPhan = outsiderLevels[2];
            suggestedBorb = outsiderLevels[3];
            suggestedPony = outsiderLevels[4];
        }

        Helpers.getElementsByDataType("suggestedOutsiderXyliqil")[0].textContent = suggestedXyl.toString();
        Helpers.getElementsByDataType("suggestedOutsiderChorgorloth")[0].textContent = suggestedChor.toString();
        Helpers.getElementsByDataType("suggestedOutsiderPhandoryss")[0].textContent = suggestedPhan.toString();
        Helpers.getElementsByDataType("suggestedOutsiderBorb")[0].textContent = suggestedBorb.toString();
        Helpers.getElementsByDataType("suggestedOutsiderPonyboy")[0].textContent = suggestedPony.toString();

        const lowAncientSoulWarning = document.getElementById("lowAncientSoulWarning");
        if (showLowAncientSoulWarning)
        {
            lowAncientSoulWarning.classList.remove("hidden");
            appInsights.trackEvent("LowAncientSouls", { ancientSouls: ancientSouls.toString() });
        }
        else
        {
            lowAncientSoulWarning.classList.add("hidden");
        }

        const missingSimulationWarning = document.getElementById("missingSimulationWarning");
        if (showMissingSimulationWarning)
        {
            missingSimulationWarning.classList.remove("hidden");
            appInsights.trackEvent("MissingSimulationData", { ancientSouls: ancientSouls.toString() });
        }
        else
        {
            missingSimulationWarning.classList.add("hidden");
        }
    }

    function getCurrentAncientLevel(stats: IMap<number>, ancient: string): number
    {
        let ancientLevel = stats["ancient" + ancient] || 0;
        if (userSettings.useEffectiveLevelForSuggestions)
        {
            ancientLevel += stats["item" + ancient] || 0;
        }

        return ancientLevel;
    }

    function getPreTranscendentSuggestedSolomonLevel(currentPrimaryAncientLevel: number, playStyle: string): number
    {
        let solomonMultiplier1: number;
        let solomonMultiplier2: number;
        switch (playStyle)
        {
            case "idle":
                solomonMultiplier1 = 1.15;
                solomonMultiplier2 = 3.25;
                break;
            case "hybrid":
                solomonMultiplier1 = 1.32;
                solomonMultiplier2 = 4.65;
                break;
            case "active":
                solomonMultiplier1 = 1.21;
                solomonMultiplier2 = 3.73;
                break;
        }

        const solomonLogFunction = userSettings.useReducedSolomonFormula
            ? Math.log10
            : Math.log;
        return currentPrimaryAncientLevel < 100
            ? currentPrimaryAncientLevel
            : Math.round(solomonMultiplier1 * Math.pow(solomonLogFunction(solomonMultiplier2 * Math.pow(currentPrimaryAncientLevel, 2)), 0.4) * Math.pow(currentPrimaryAncientLevel, 0.8));
    }

    function getTotalAncientCost(suggestedLevels: IMap<number>, stats: IMap<number>): number
    {
        let cost = 0;
        const chorgorlothLevel = stats["outsiderChorgorloth"] || 0;
        const ancientCostMultiplier = Math.pow(0.95, chorgorlothLevel);

        for (let ancient in suggestedLevels)
        {
            const suggestedLevel = suggestedLevels[ancient];
            const currentLevel = getCurrentAncientLevel(stats, ancient);

            // If the ancient is over-leveled, no cost
            if (suggestedLevel < currentLevel)
            {
                continue;
            }

            const costFormula = ancientCostFormulas[ancient];
            if (!costFormula)
            {
                continue;
            }

            cost += Math.ceil((costFormula(suggestedLevel) - costFormula(currentLevel)) * ancientCostMultiplier);
        }

        return cost;
    }

    function getAncientCostFormulas(): IMap<(level: number) => number>
    {
        const ancientCosts: IMap<(level: number) => number> = {};

        for (const ancientId in ancientsData)
        {
            const ancient = ancientsData[ancientId];

            let ancientCost: (level: number) => number;
            switch (ancient.levelCostFormula)
            {
                case "one":
                    ancientCost = (n: number) => n;
                    break;
                case "linear":
                    ancientCost = (n: number) => n * (n + 1) / 2;
                    break;
                case "polynomial1_5":
                    ancientCost = (n: number) =>
                    {
                        // Approximate above a certain level for perf
                        // Formula taken from https://github.com/superbob/clicker-heroes-1.0-hsoptimizer/blob/335f13b7304627065a4e515edeb3fb3c4e08f8ad/src/app/components/maths/maths.service.js
                        if (n > 100)
                        {
                            return Math.ceil(2 * Math.pow(n, 2.5) / 5 + Math.pow(n, 1.5) / 2 + Math.pow(n, 0.5) / 8 + Math.pow(n, - 1.5) / 1920);
                        }

                        let cost = 0;
                        for (let i = 1; i <= n; i++)
                        {
                            cost += Math.pow(i, 1.5);
                        }

                        return Math.ceil(cost);
                    };
                    break;
                case "exponential":
                    ancientCost = (n: number) => Math.pow(2, n + 1) - 1;
                    break;
                default:
                    ancientCost = () => 0;
            }

            ancientCosts[ancient.shortName] = ancientCost;
        }

        return ancientCosts;
    }

    function getOptimalOutsiderLevels(): IMap<[number, number, number, number, number]>
    {
        return {
            // From https://docs.google.com/spreadsheets/d/1m09HoNiLW-7t96gzguG9tU_HHaRrDrtMpAoAuukLB4w
            "30": [7, 0, 0, 0, 23],
            "31": [7, 0, 1, 0, 23],
            "32": [7, 0, 1, 0, 24],
            "33": [7, 0, 1, 0, 25],
            "34": [6, 0, 1, 0, 27],
            "35": [6, 0, 1, 0, 28],
            "36": [7, 0, 2, 0, 26],
            "37": [7, 1, 2, 0, 26],
            "38": [7, 1, 2, 0, 27],
            "39": [7, 2, 2, 0, 27],
            // From https://docs.google.com/spreadsheets/d/1LlW5ZJUY5QuQlkdk1FRWrsOeB8PuWQwig9L-ZyRUekY/edit#gid=686505719
            "40": [6, 2, 3, 0, 26],
            "41": [6, 2, 3, 0, 27],
            "42": [6, 1, 4, 0, 25],
            "43": [6, 2, 4, 0, 25],
            "44": [7, 1, 4, 1, 25],
            "45": [7, 3, 4, 0, 25],
            "46": [6, 4, 4, 1, 25],
            "47": [6, 6, 4, 0, 25],
            "48": [6, 6, 4, 0, 26],
            "49": [6, 4, 5, 0, 24],
            "50": [6, 4, 5, 0, 25],
            "51": [6, 5, 5, 1, 24],
            "52": [6, 7, 5, 0, 24],
            "53": [6, 7, 5, 0, 25],
            "54": [7, 9, 5, 0, 23],
            "55": [6, 10, 5, 0, 24],
            "56": [6, 9, 5, 1, 25],
            "57": [7, 10, 5, 1, 24],
            "58": [6, 6, 6, 2, 23],
            "59": [6, 8, 6, 2, 22],
            "60": [6, 9, 6, 2, 22],
            "61": [6, 10, 6, 1, 23],
            "62": [6, 10, 6, 2, 23],
            "63": [6, 10, 6, 2, 24],
            "64": [6, 10, 6, 3, 24],
            "65": [7, 9, 6, 3, 25],
            "66": [7, 10, 6, 3, 25],
            "67": [6, 8, 7, 2, 23],
            "68": [6, 10, 7, 1, 23],
            "69": [6, 10, 7, 2, 23],
            "70": [6, 10, 7, 2, 24],
            "71": [6, 10, 7, 3, 24],
            "72": [6, 10, 7, 3, 25],
            "73": [7, 10, 7, 3, 25],
            "74": [6, 9, 8, 2, 21],
            "75": [6, 8, 8, 2, 23],
            "76": [6, 10, 8, 2, 22],
            "77": [6, 8, 8, 4, 23],
            "78": [6, 9, 8, 3, 24],
            "79": [6, 10, 8, 3, 24],
            "80": [6, 10, 8, 4, 24],
            "81": [6, 10, 8, 4, 25],
            "82": [6, 10, 8, 3, 27],
            "83": [6, 10, 8, 4, 27],
            "84": [7, 10, 8, 4, 27],
            "85": [7, 10, 8, 5, 27],
            "86": [6, 9, 9, 3, 23],
            "87": [6, 10, 9, 3, 23],
            "88": [6, 10, 9, 4, 23],
            "89": [7, 10, 9, 4, 23],
            "90": [7, 9, 9, 4, 25],
            "91": [7, 10, 9, 5, 24],
            "92": [8, 10, 9, 6, 23],
            "93": [7, 10, 9, 5, 26],
            "94": [7, 10, 9, 6, 26],
            "95": [7, 10, 9, 7, 26],
            "96": [9, 10, 9, 7, 25],
            "97": [10, 10, 9, 7, 25],
            "98": [9, 10, 9, 8, 26],
            "99": [7, 10, 10, 6, 21],
            "100": [7, 10, 10, 7, 21],
            "101": [6, 10, 10, 7, 23],
            "102": [7, 10, 10, 7, 23],
            "103": [7, 10, 10, 8, 23],
            "104": [7, 10, 10, 8, 24],
            "105": [8, 10, 10, 9, 23],
            "106": [8, 10, 10, 10, 23],
            "107": [7, 10, 10, 10, 25],
            "108": [7, 10, 10, 11, 25],
            "109": [8, 10, 10, 11, 25],
            "110": [8, 10, 10, 11, 26],
            "111": [8, 10, 10, 11, 27],
            "112": [8, 10, 10, 11, 28],
            "113": [8, 10, 10, 13, 27],
            "114": [8, 10, 10, 14, 27],
            "115": [8, 10, 10, 14, 28],
            "116": [7, 10, 11, 12, 21],
            "117": [7, 10, 11, 12, 22],
            "118": [7, 10, 11, 12, 23],
            "119": [7, 10, 11, 14, 22],
            "120": [7, 10, 11, 13, 24],
            "121": [7, 10, 11, 14, 24],
            "122": [8, 10, 11, 14, 24],
            "123": [7, 10, 11, 14, 26],
            "124": [7, 10, 11, 15, 26],
            "125": [8, 10, 11, 15, 26],
            "126": [8, 10, 11, 15, 27],
            "127": [8, 10, 11, 17, 26],
            "128": [8, 10, 11, 18, 26],
            "129": [9, 10, 11, 17, 27],
            "130": [8, 10, 11, 19, 27],
            "131": [8, 10, 11, 18, 29],
            "132": [9, 10, 11, 18, 29],
            "133": [8, 10, 11, 20, 29],
            "134": [8, 10, 11, 20, 30],
            "135": [7, 10, 12, 16, 24],
            "136": [8, 10, 12, 16, 24],
            "137": [7, 10, 12, 16, 26],
            "138": [8, 10, 12, 16, 26],
            "139": [7, 10, 12, 19, 25],
            "140": [8, 10, 12, 19, 25],
            "141": [8, 10, 12, 18, 27],
            "142": [9, 10, 12, 19, 26],
            "143": [7, 10, 12, 23, 25],
            "144": [8, 10, 12, 23, 25],
            "145": [7, 10, 12, 24, 26],
            "146": [9, 10, 12, 23, 26],
            "147": [9, 10, 12, 23, 27],
            "148": [10, 10, 12, 23, 27],
            "149": [10, 10, 12, 24, 27],
            "150": [11, 10, 12, 25, 26],
            "151": [9, 10, 12, 25, 29],
            "152": [9, 10, 12, 26, 29],
            "153": [9, 10, 12, 28, 28],
            "154": [9, 10, 12, 28, 29],
            "155": [10, 10, 12, 26, 31],
            "156": [11, 10, 12, 27, 30],
            "157": [9, 10, 12, 29, 31],
            "158": [9, 10, 12, 30, 31],
            "159": [6, 10, 13, 26, 26],
            "160": [8, 10, 13, 25, 26],
            "161": [8, 10, 13, 26, 26],
            "162": [8, 10, 13, 28, 25],
            "163": [8, 10, 13, 28, 26],
            "164": [8, 10, 13, 29, 26],
            "165": [8, 10, 13, 30, 26],
            "166": [8, 10, 13, 30, 27],
            "167": [8, 10, 13, 31, 27],
            "168": [9, 10, 13, 31, 27],
            "169": [9, 10, 13, 31, 28],
            "170": [9, 10, 13, 32, 28],
            "171": [9, 10, 13, 34, 27],
            "172": [8, 10, 13, 34, 29],
            "173": [8, 10, 14, 26, 24],
            "174": [8, 10, 14, 26, 25],
            "175": [9, 10, 14, 25, 26],
            "176": [7, 10, 14, 27, 27],
            "177": [8, 10, 14, 29, 25],
            "178": [9, 10, 14, 30, 24],
            "179": [10, 10, 14, 30, 24],
            "180": [8, 10, 14, 31, 26],
            "181": [9, 10, 14, 30, 27],
            "182": [8, 10, 14, 31, 28],
            "183": [9, 10, 14, 31, 28],
            "184": [8, 10, 14, 32, 29],
            "185": [9, 10, 14, 33, 28],
            "186": [9, 10, 14, 34, 28],
            "187": [10, 10, 14, 35, 27],
            "188": [9, 10, 14, 34, 30],
            "189": [8, 10, 14, 36, 30],
            "190": [9, 10, 14, 36, 30],
            "191": [10, 10, 14, 37, 29],
            "192": [9, 10, 14, 39, 29],
            "193": [10, 10, 14, 40, 28],
            "194": [6, 10, 15, 29, 29],
            "195": [7, 10, 15, 30, 28],
            "196": [9, 10, 15, 32, 25],
            "197": [9, 10, 15, 33, 25],
            "198": [8, 10, 15, 34, 26],
            "199": [10, 10, 15, 33, 26],
            "200": [8, 10, 15, 36, 26],
            "201": [8, 10, 15, 37, 26],
            "202": [8, 10, 15, 38, 26],
            "203": [8, 10, 15, 39, 26],
            "204": [8, 10, 15, 37, 29],
            "205": [8, 10, 15, 38, 29],
            "206": [8, 10, 15, 39, 29],
            "207": [8, 10, 15, 40, 29],
            "208": [10, 10, 15, 40, 28],
            "209": [9, 10, 15, 42, 28],
            "210": [9, 10, 15, 44, 27],
            // Goes every 10 from here
            "220": [10, 10, 15, 48, 32],
            "230": [9, 10, 16, 44, 31],
            "240": [9, 10, 16, 54, 31],
            "250": [9, 10, 17, 47, 31],
            "260": [9, 10, 17, 57, 31],
            "270": [10, 10, 17, 66, 31],
            "280": [11, 10, 17, 73, 33],
            "290": [12, 10, 17, 80, 35],
            "300": [13, 10, 18, 72, 34],
            "310": [13, 10, 18, 82, 34],
            "320": [10, 10, 19, 76, 34],
            "330": [10, 10, 19, 86, 34],
            "340": [10, 10, 19, 94, 36],
            "350": [9, 10, 20, 89, 32],
            "360": [10, 10, 20, 96, 34],
            "370": [11, 10, 20, 104, 35],
            "380": [11, 10, 20, 113, 36],
            "390": [12, 10, 20, 120, 38],
            "400": [12, 10, 21, 109, 38],
            "410": [12, 10, 21, 119, 38],
            "420": [13, 10, 21, 128, 38],
            "430": [13, 13, 21, 133, 37],
            "440": [13, 12, 21, 143, 39],
            "450": [12, 13, 21, 151, 40],
            "460": [16, 12, 21, 159, 40],
            "470": [12, 18, 21, 163, 38],
            "480": [14, 18, 21, 171, 38],
            "490": [15, 20, 21, 177, 37],
            "500": [17, 19, 22, 164, 38],
            // Goes every 100 from here
            "600": [11, 20, 25, 192, 42],
            "700": [14, 20, 25, 264, 67],
            "800": [11, 20, 26, 341, 67],
            "900": [18, 24, 28, 374, 60],
            "1000": [15, 31, 29, 432, 54],
            "1100": [18, 28, 30, 503, 60],
            "1200": [18, 30, 31, 569, 57],
            "1300": [25, 30, 31, 637, 82],
            "1400": [23, 40, 33, 645, 71],
            "1500": [20, 41, 34, 711, 69],
        };
    }

    const uploadId = Helpers.getElementsByDataType("uploadId")[0].textContent;

    // Get upload data
    $.ajax({
        url: "/api/uploads/" + uploadId,
    })
        .done(handleSuccess)
        .fail(displayFailure);

    // Set up delete button
    $("#deleteUpload").click(() =>
    {
        $.ajax({
            method: "DELETE",
            url: "/api/uploads/" + uploadId,
        })
            .done(() =>
            {
                window.location.href = "/dashboard";
            })
            .fail(displayFailure);
    });

    // Set up suggestion type handlers
    $("input[name='SuggestionType']").change(hydrateAncientSuggestions);
    $("#UseSoulsFromAscension").click(hydrateAncientSuggestions);
}
