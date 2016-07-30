namespace Calculator
{
    "use strict";

    function handleSuccess(upload: IUpload): void
    {
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

            // Add click to copy handlers
            const suggestedLevelsTables = Helpers.getElementsByDataType("suggestedLevels");
            if (suggestedLevelsTables)
            {
                for (let i = 0; i < suggestedLevelsTables.length; i++)
                {
                    const table = suggestedLevelsTables[i] as HTMLTableElement;
                    const rows = table.rows;

                    // Skip the first row since it's the header.
                    for (let j = 1; j < rows.length; j++)
                    {
                        const row = rows[j] as HTMLTableRowElement;
                        const diffCell = row.cells[row.cells.length - 1];
                        diffCell.addEventListener("click", function (): void
                        {
                            Helpers.copyToClipboard(diffCell.textContent);
                        });
                    }
                }
            }

            calculateAncientSuggestions(upload.stats, upload.playStyle);

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

        // Handle play style
        const primaryAncient = upload.playStyle === "active"
            ? "Fragsworth"
            : "Siyalatas";

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
        // Normalize the value
        suggestedLevel = Math.max(Math.round(suggestedLevel), 0);

        hydrateStat(stats, "suggested" + ancient, suggestedLevel);
        hydrateStat(stats, "diff" + ancient, suggestedLevel - getCurrentAncientLevel(stats, ancient));
    }

    function hideAncientSuggestion(ancient: string): void
    {
        const statElements = Helpers.getElementsByDataType("suggested" + ancient);
        if (statElements)
        {
            for (let i = 0; i < statElements.length; i++)
            {
                statElements[i].parentElement.classList.add("hidden");
            }
        }
    }

    function displayFailure(): void
    {
        // BUGBUG 51: Create Loading and Failure states for ajax loading
    }

    function calculateAncientSuggestions(stats: IMap<number>, playStyle: string): void
    {
        const primaryAncient = playStyle === "active" ? "Fragsworth" : "Siyalatas";

        const currentPrimaryAncientLevel = getCurrentAncientLevel(stats, primaryAncient);
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
        hydrateAncientSuggestion(stats, "Argaiv", currentPrimaryAncientLevel);
        hydrateAncientSuggestion(stats, "Atman", (2.832 * lnPrimary) - (1.416 * lnAlpha) - (1.416 * Math.log((4 / 3) - Math.pow(Math.E, -0.013 * currentAtmanLevel))) - 6.613);
        hydrateAncientSuggestion(stats, "Bubos", (2.8 * lnPrimary) - (1.4 * Math.log(1 + Math.pow(Math.E, -0.02 * currentBubosLevel))) - 5.94);
        hydrateAncientSuggestion(stats, "Chronos", (2.75 * lnPrimary) - (1.375 * Math.log(2 - Math.pow(Math.E, -0.034 * currentChronosLevel))) - 5.1);
        hydrateAncientSuggestion(stats, "Dogcog", (2.844 * lnPrimary) - (1.422 * Math.log((1 / 99) + Math.pow(Math.E, -0.01 * currentDogcogLevel))) - 7.232);
        hydrateAncientSuggestion(stats, "Dora", (2.877 * lnPrimary) - (1.4365 * Math.log((100 / 99) - Math.pow(Math.E, -0.002 * currentDoraLevel))) - 9.63);
        hydrateAncientSuggestion(stats, "Fortuna", (2.875 * lnPrimary) - (1.4375 * Math.log((10 / 9) - Math.pow(Math.E, -0.0025 * currentFortunaLevel))) - 9.3);
        hydrateAncientSuggestion(stats, "Kumawakamaru", (2.844 * lnPrimary) - (1.422 * lnAlpha) - (1.422 * Math.log(0.25 + Math.pow(Math.E, -0.001 * currentKumaLevel))) - 7.014);
        const suggestedGoldLevel = currentPrimaryAncientLevel * 0.926;
        hydrateAncientSuggestion(stats, "Libertas", suggestedGoldLevel);
        hydrateAncientSuggestion(stats, "Mammon", suggestedGoldLevel);
        hydrateAncientSuggestion(stats, "Mimzee", suggestedGoldLevel);
        hydrateAncientSuggestion(stats, "Morgulis", currentPrimaryAncientLevel * currentPrimaryAncientLevel);
        hydrateAncientSuggestion(stats, "Solomon", stats["transcendentPower"] > 0
            ? Math.pow(currentPrimaryAncientLevel, 0.8) / Math.pow(alpha, 0.4)
            : getPreTranscendentSuggestedSolomonLevel(currentPrimaryAncientLevel, playStyle));

        // Math per play style
        switch (playStyle)
        {
            case "idle":
                hideAncientSuggestion("Bhaal");
                hideAncientSuggestion("Fragsworth");
                hideAncientSuggestion("Juggernaut");
                break;
            case "hybrid":
                const hybridRatioReciprocal = 1 / userSettings.hybridRatio;
                const suggestedActiveLevelUnrounded = hybridRatioReciprocal * currentPrimaryAncientLevel;
                const suggestedActiveLevel = Math.round(suggestedActiveLevelUnrounded);
                hydrateAncientSuggestion(stats, "Bhaal", suggestedActiveLevel);
                hydrateAncientSuggestion(stats, "Fragsworth", suggestedActiveLevel);
                hydrateAncientSuggestion(stats, "Juggernaut", Math.pow(suggestedActiveLevelUnrounded, 0.8));
                break;
            case "active":
                hydrateAncientSuggestion(stats, "Bhaal", currentPrimaryAncientLevel);
                hydrateAncientSuggestion(stats, "Juggernaut", Math.pow(currentPrimaryAncientLevel, 0.8));
                hideAncientSuggestion("Libertas");
                hideAncientSuggestion("Siyalatas");
                break;
        }
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
}
