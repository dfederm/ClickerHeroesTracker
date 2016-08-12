namespace Calculator
{
    "use strict";

    // An index for quick lookup of ancient cost formulas.
    // Each formula gets the sum of the cost of the ancient from 1 to N.
    const ancientCostFormulas = getAncientCostFormulas();

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

            // Default Xyl to something reasonable for the playstyle
            const ancientSouls = upload.stats["totalAncientSouls"];
            let suggestedXylLevel = 0;
            if (upload.playStyle === "idle")
            {
                // RoT says "at max 20% of total", but I prefer just 6
                suggestedXylLevel = 6;
            }
            else if (upload.playStyle === "hybrid")
            {
                suggestedXylLevel = Math.round(0.05 * ancientSouls);
            }
            else if (upload.playStyle === "active")
            {
                // RoT says 0-3, let's guess 3.
                suggestedXylLevel = 3;
            }

            // Only suggest what they can buy
            suggestedXylLevel = Math.min(ancientSouls, suggestedXylLevel);

            const suggestedXylElement = Helpers.getElementsByDataType("suggestedOutsiderXyliqil")[0] as HTMLInputElement;
            suggestedXylElement.value = suggestedXylLevel.toString();
            suggestedXylElement.addEventListener("change", calculateOutsiderSuggestions);

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
        const stats = lastUpload.stats;
        if (!stats)
        {
            return;
        }

        const primaryAncient = lastUpload.playStyle === "active"
            ? "Fragsworth"
            : "Siyalatas";

        let suggestedLevels: IMap<number>;

        const suggestionType = $("input[name='SuggestionType']:checked").val();
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
            suggestedLevels[primaryAncient] = primaryAncientLevel;
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
    }

    function calculateAncientSuggestions(currentPrimaryAncientLevel?: number): IMap<number>
    {
        const stats = lastUpload.stats;
        const playStyle = lastUpload.playStyle;

        const primaryAncient = playStyle === "active" ? "Fragsworth" : "Siyalatas";
        if (currentPrimaryAncientLevel === undefined)
        {
            currentPrimaryAncientLevel = getCurrentAncientLevel(stats, primaryAncient);
        }

        const suggestedLevels: IMap<number> = {};

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
        suggestedLevels["Libertas"] = suggestedLevels["Mammon"] = suggestedLevels["Mimzee"] = currentPrimaryAncientLevel * 0.926;
        suggestedLevels["Morgulis"] = currentPrimaryAncientLevel * currentPrimaryAncientLevel;
        suggestedLevels["Solomon"] = stats["transcendentPower"] > 0
            ? Math.pow(currentPrimaryAncientLevel, 0.8) / Math.pow(alpha, 0.4)
            : getPreTranscendentSuggestedSolomonLevel(currentPrimaryAncientLevel, playStyle);

        // Math per play style
        switch (playStyle)
        {
            case "idle":
                break;
            case "hybrid":
                const hybridRatioReciprocal = 1 / userSettings.hybridRatio;
                const suggestedActiveLevelUnrounded = hybridRatioReciprocal * currentPrimaryAncientLevel;
                suggestedLevels["Bhaal"] = suggestedLevels["Fragsworth"] = Math.round(suggestedActiveLevelUnrounded);
                suggestedLevels["Juggernaut"] = Math.pow(suggestedActiveLevelUnrounded, 0.8);
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
        let ancientSouls = parseInt(Helpers.getElementsByDataType("totalAncientSouls")[0].textContent);
        if (ancientSouls === 0)
        {
            return;
        }

        const xylElement = Helpers.getElementsByDataType("suggestedOutsiderXyliqil")[0] as HTMLInputElement;
        let xylLevel = parseInt(xylElement.value);
        if (xylLevel < 0 || isNaN(xylLevel))
        {
            xylLevel = 0;
            xylElement.value = xylLevel.toString();
        }

        if (xylLevel > ancientSouls)
        {
            xylLevel = ancientSouls;
            xylElement.value = xylLevel.toString();
        }

        // Cost of Xyl
        ancientSouls -= xylLevel;

        // The index of this array is 1 less than the level Phan should be if the value at that index can just barely be bought.
        const phanTable = [3, 10, 21, 36, 54, 60, 67, 75, 84, 94, 104, 117, 129, 143, 158, 174, 190, 208, 228];
        let suggestedPhan = 0;
        for ( ; suggestedPhan < phanTable.length; suggestedPhan++)
        {
            // If we can no longer afford it, break.
            if (phanTable[suggestedPhan] > ancientSouls)
            {
                break;
            }
        }

        // Cost of Phan
        ancientSouls -= (1 + suggestedPhan) * suggestedPhan / 2;

        // Put 1/10 of your remaining AS in Borb (round as you like, probably round up).
        let suggestedBorb = Math.ceil(0.1 * ancientSouls);
        ancientSouls -= suggestedBorb;

        // Get Pony to level 19.
        let suggestedPony = Math.min(ancientSouls, 19);
        ancientSouls -= suggestedPony;

        // Get Chor'gorloth to level 10.
        let suggestedChor = Math.min(ancientSouls, 10);
        ancientSouls -= suggestedChor;

        // Get Borb to 10 now.
        if (ancientSouls > 0)
        {
            ancientSouls += suggestedBorb;
            suggestedBorb = Math.min(ancientSouls, 10);
            ancientSouls -= suggestedBorb;
        }

        //  If you still have AS left over, put them equally into Pony and Borb (e.g.if 4 AS left over: Pony to 21 and Borb to 12 total)
        if (ancientSouls > 0)
        {
            // For uneven AS, preferring Borb. Why not?
            suggestedPony += Math.floor(ancientSouls / 2);
            suggestedBorb += Math.ceil(ancientSouls / 2);
        }

        Helpers.getElementsByDataType("suggestedOutsiderChorgorloth")[0].textContent = suggestedChor.toString();
        Helpers.getElementsByDataType("suggestedOutsiderPhandoryss")[0].textContent = suggestedPhan.toString();
        Helpers.getElementsByDataType("suggestedOutsiderBorb")[0].textContent = suggestedBorb.toString();
        Helpers.getElementsByDataType("suggestedOutsiderPonyboy")[0].textContent = suggestedPony.toString();
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
                    ancientCost = (n: number) => n;
                    break;
                default:
                    ancientCost = (n: number) => 0;
            }

            ancientCosts[ancient.shortName] = ancientCost;
        }

        return ancientCosts;
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
