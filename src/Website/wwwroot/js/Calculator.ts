namespace Calculator
{
    "use strict";

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
                        Helpers.copyToClipboard(statValue.toString());
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
                    ancientCost = (n: number) => 0;
            }

            ancientCosts[ancient.shortName] = ancientCost;
        }

        return ancientCosts;
    }

    function getOptimalOutsiderLevels(): IMap<[number, number, number, number, number]>
    {
        return {
            // From https://docs.google.com/spreadsheets/d/1m09HoNiLW-7t96gzguG9tU_HHaRrDrtMpAoAuukLB4w/edit#gid=0
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
            // From https://docs.google.com/spreadsheets/d/1LlW5ZJUY5QuQlkdk1FRWrsOeB8PuWQwig9L-ZyRUekY/edit#gid=1843865711
            "40": [6, 5, 2, 0, 26],
            "41": [6, 4, 3, 0, 25],
            "42": [5, 4, 3, 0, 27],
            "43": [5, 6, 3, 0, 26],
            "44": [5, 6, 3, 0, 27],
            "45": [5, 5, 4, 0, 25],
            "46": [5, 5, 4, 0, 26],
            "47": [5, 7, 4, 0, 25],
            "48": [6, 7, 4, 0, 25],
            "49": [5, 10, 4, 0, 24],
            "50": [5, 5, 5, 0, 25],
            "51": [5, 6, 5, 0, 25],
            "52": [5, 9, 5, 0, 23],
            "53": [5, 8, 5, 0, 25],
            "54": [5, 10, 5, 0, 24],
            "55": [5, 10, 5, 0, 25],
            "56": [5, 10, 5, 0, 26],
            "57": [6, 10, 5, 0, 26],
            "58": [5, 9, 6, 0, 23],
            "59": [5, 9, 6, 0, 24],
            "60": [5, 10, 6, 0, 24],
            "61": [6, 10, 6, 0, 24],
            "62": [5, 10, 6, 0, 26],
            "63": [5, 10, 6, 0, 27],
            "64": [6, 10, 6, 0, 27],
            "65": [5, 8, 7, 0, 24],
            "66": [5, 10, 7, 0, 23],
            "67": [5, 10, 7, 0, 24],
            "68": [5, 10, 7, 0, 25],
            "69": [5, 10, 7, 0, 26],
            "70": [6, 10, 7, 0, 26],
            "71": [6, 10, 7, 0, 27],
            "72": [5, 9, 8, 0, 22],
            "73": [5, 9, 8, 0, 23],
            "74": [5, 10, 8, 0, 23],
            "75": [6, 10, 8, 0, 23],
            "76": [6, 10, 8, 0, 24],
            "77": [5, 10, 8, 0, 26],
            "78": [6, 10, 8, 0, 26],
            "79": [6, 10, 8, 0, 27],
            "80": [5, 8, 9, 0, 22],
            "81": [5, 9, 9, 0, 22],
            "82": [5, 8, 9, 0, 24],
            "83": [5, 9, 9, 0, 24],
            "84": [5, 10, 9, 0, 24],
            "85": [5, 10, 9, 0, 25],
            "86": [5, 10, 9, 0, 26],
            "87": [6, 10, 9, 0, 26],
            "88": [5, 10, 9, 0, 28],
            "89": [6, 10, 9, 1, 27],
            "90": [5, 8, 10, 0, 22],
            "91": [5, 9, 10, 0, 22],
            "92": [5, 10, 10, 0, 22],
            "93": [5, 9, 10, 0, 24],
            "94": [5, 10, 10, 0, 24],
            "95": [5, 10, 10, 1, 24],
            "96": [6, 10, 10, 0, 25],
            "97": [5, 10, 10, 0, 27],
            "98": [6, 10, 10, 0, 27],
            "99": [6, 10, 10, 1, 27],
            "100": [6, 10, 10, 1, 28],
            "101": [5, 9, 11, 0, 21],
            "102": [5, 10, 11, 0, 21],
            "103": [5, 10, 11, 0, 22],
            "104": [5, 10, 11, 0, 23],
            "105": [5, 10, 11, 0, 24],
            "106": [6, 10, 11, 1, 23],
            "107": [5, 10, 11, 1, 25],
            "108": [6, 10, 11, 1, 25],
            "109": [6, 10, 11, 3, 24],
            "110": [6, 10, 11, 2, 26],
            "111": [6, 10, 11, 2, 27],
            "112": [6, 10, 11, 3, 27],
            "113": [5, 7, 12, 1, 22],
            "114": [5, 7, 12, 1, 23],
            "115": [5, 10, 12, 2, 20],
            "116": [5, 9, 12, 1, 23],
            "117": [5, 9, 12, 3, 22],
            "118": [5, 10, 12, 2, 23],
            "119": [5, 10, 12, 3, 23],
            "120": [6, 10, 12, 3, 23],
            "121": [6, 10, 12, 3, 24],
            "122": [5, 10, 12, 3, 26],
            "123": [6, 10, 12, 4, 25],
            "124": [6, 10, 12, 4, 26],
            "125": [6, 10, 12, 4, 27],
            "126": [6, 10, 12, 5, 27],
            "127": [5, 7, 13, 2, 22],
            "128": [5, 9, 13, 2, 21],
            "129": [5, 9, 13, 3, 21],
            "130": [5, 9, 13, 3, 22],
            "131": [5, 9, 13, 3, 23],
            "132": [6, 10, 13, 3, 22],
            "133": [5, 10, 13, 4, 23],
            "134": [6, 10, 13, 4, 23],
            "135": [6, 10, 13, 4, 24],
            "136": [6, 10, 13, 5, 24],
            "137": [6, 10, 13, 5, 25],
            "138": [7, 10, 13, 5, 25],
            "139": [6, 10, 13, 5, 27],
            "140": [6, 10, 13, 6, 27],
            "141": [6, 10, 13, 7, 27],
            "142": [7, 10, 13, 6, 28],
            "143": [5, 7, 14, 5, 21],
            "144": [5, 7, 14, 5, 22],
            "145": [5, 8, 14, 6, 21],
            "146": [5, 8, 14, 6, 22],
            "147": [5, 9, 14, 6, 22],
            "148": [5, 10, 14, 6, 22],
            "149": [5, 10, 14, 7, 22],
            "150": [6, 10, 14, 7, 22],
            "151": [6, 10, 14, 8, 22],
            "152": [6, 10, 14, 8, 23],
            "153": [6, 10, 14, 8, 24],
            "154": [6, 10, 14, 9, 24],
            "155": [7, 10, 14, 8, 25],
            "156": [6, 10, 14, 9, 26],
            "157": [6, 10, 14, 10, 26],
            "158": [7, 10, 14, 10, 26],
            "159": [6, 10, 14, 10, 28],
            "160": [7, 10, 14, 11, 27],
            "161": [7, 10, 14, 11, 28],
            "162": [7, 10, 14, 12, 28],
            "163": [8, 10, 14, 12, 28],
            "164": [6, 7, 15, 9, 22],
            "165": [6, 9, 15, 9, 21],
            "166": [6, 9, 15, 9, 22],
            "167": [5, 10, 15, 9, 23],
            "168": [6, 10, 15, 9, 23],
            "169": [6, 10, 15, 10, 23],
            "170": [7, 10, 15, 10, 23],
            "171": [6, 9, 15, 11, 25],
            "172": [7, 10, 15, 11, 24],
            "173": [7, 10, 15, 12, 24],
            "174": [7, 10, 15, 13, 24],
            "175": [7, 10, 15, 14, 24],
            "176": [7, 10, 15, 14, 25],
            "177": [7, 10, 15, 14, 26],
            "178": [8, 10, 15, 13, 27],
            "179": [8, 10, 15, 14, 27],
            "180": [7, 10, 15, 15, 28],
            "181": [7, 10, 15, 15, 29],
            "182": [8, 10, 15, 15, 29],
            "183": [8, 10, 15, 16, 29],
            "184": [7, 10, 15, 17, 30],
            "185": [7, 10, 15, 20, 28],
            "186": [8, 10, 15, 19, 29],
            "187": [8, 10, 15, 19, 30],
            "188": [7, 10, 16, 14, 21],
            "189": [7, 8, 16, 17, 21],
            "190": [7, 9, 16, 15, 23],
            "191": [6, 10, 16, 16, 23],
            "192": [6, 10, 16, 19, 21],
            "193": [7, 10, 16, 18, 22],
            "194": [7, 10, 16, 18, 23],
            "195": [7, 10, 16, 18, 24],
            "196": [7, 10, 16, 17, 26],
            "197": [7, 10, 16, 19, 25],
            "198": [8, 10, 16, 19, 25],
            "199": [8, 10, 16, 19, 26],
            "200": [8, 10, 16, 21, 25],
            "201": [8, 10, 16, 21, 26],
            "202": [8, 10, 16, 22, 26],
            "203": [7, 10, 16, 23, 27],
            "204": [8, 10, 16, 22, 28],
            "205": [8, 10, 16, 25, 26],
            "206": [8, 10, 16, 25, 27],
            "207": [8, 10, 16, 25, 28],
            "208": [8, 10, 16, 25, 29],
            "209": [9, 10, 16, 25, 29],
            "210": [9, 10, 16, 26, 29],
            // Goes every 10 from here
            "220": [8, 10, 17, 26, 23],
            "230": [9, 10, 17, 31, 27],
            "240": [10, 10, 17, 40, 27],
            "250": [9, 10, 18, 34, 26],
            "260": [9, 10, 18, 41, 29],
            "270": [11, 10, 18, 46, 32],
            "280": [10, 10, 19, 42, 28],
            "290": [9, 10, 19, 54, 27],
            "300": [12, 10, 19, 61, 27],
            "310": [10, 10, 20, 55, 25],
            "320": [10, 10, 20, 64, 26],
            "330": [12, 10, 20, 69, 29],
            "340": [10, 10, 20, 80, 30],
            "350": [10, 10, 20, 84, 36],
            "360": [13, 10, 21, 75, 31],
            "370": [13, 10, 21, 82, 34],
            "380": [13, 10, 21, 92, 34],
            "390": [15, 11, 21, 96, 36],
            "400": [12, 10, 22, 95, 30],
            "410": [12, 10, 22, 111, 24],
            "420": [16, 11, 22, 101, 38],
            "430": [16, 10, 23, 95, 33],
            "440": [17, 10, 23, 109, 28],
            "450": [17, 16, 23, 106, 29],
            "460": [16, 10, 24, 101, 33],
            "470": [19, 10, 24, 108, 33],
            "480": [17, 13, 24, 112, 35],
            "490": [16, 10, 25, 114, 25],
            "500": [16, 10, 25, 121, 28],
            // Goes every 100 from here
            "600": [18, 10, 28, 137, 29],
            "700": [20, 3, 31, 154, 27],
            "800": [21, 10, 34, 145, 29],
            "900": [24, 10, 36, 174, 26],
            "1000": [19, 10, 38, 207, 23],
            "1100": [22, 3, 41, 189, 25],
            "1200": [23, 10, 43, 198, 23],
            "1300": [25, 10, 45, 205, 25],
            "1400": [25, 10, 47, 211, 26],
            "1500": [32, 10, 48, 257, 25],
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
