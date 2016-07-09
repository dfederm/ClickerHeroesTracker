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

        const uploadContentElements = Helpers.getElementsByDataType("uploadContent");
        if (upload.uploadContent)
        {
            for (let i = 0; i < uploadContentElements.length; i++)
            {
                uploadContentElements[i].textContent = upload.uploadContent;
            }
        }

        const hasOutsiderData = upload.stats && upload.stats.hasOwnProperty("totalAncientSouls");
        const hasSimulationData = upload.stats && upload.stats.hasOwnProperty("optimalLevel");

        if (upload.stats)
        {
            for (let statType in upload.stats)
            {
                hydrateStat(upload.stats, statType, upload.stats[statType]);
            }

            // Remove the stats that didn't have values
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
                        const cell = row.cells[2] as HTMLTableCellElement;
                        const dataType = cell.getAttribute("data-type");
                        if (!upload.stats.hasOwnProperty(dataType) && cell.textContent === "0")
                        {
                            row.classList.add("hidden");
                        }
                    }
                }
            }

            // Show outsider suggestions
            if (hasOutsiderData)
            {
                // Default Xyl to something reasonable for the playstyle
                const ancientSouls = getAncientSouls();
                let suggestedXylLevel = 0;
                if (userSettings.playStyle === "idle")
                {
                    // RoT says "at max 20% of total", but I prefer just 6
                    suggestedXylLevel = 6;
                }
                else if (userSettings.playStyle === "hybrid")
                {
                    suggestedXylLevel = Math.round(0.05 * ancientSouls);
                }
                else if (userSettings.playStyle === "active")
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
            }
            else
            {
                // Show the correct suggested ancient level text
                const textElements = Helpers.getElementsByDataType("suggestedLevelsText");
                for (let i = 0; i < textElements.length; i++)
                {
                    textElements[i].classList.add("hidden");
                }

                const legacyTextElements = Helpers.getElementsByDataType("suggestedLevelsTextLegacy");
                for (let i = 0; i < legacyTextElements.length; i++)
                {
                    legacyTextElements[i].classList.remove("hidden");
                }

                const solomonTooltipElements = Helpers.getElementsByDataType("solomonTooltip");
                for (let i = 0; i < solomonTooltipElements.length; i++)
                {
                    solomonTooltipElements[i].classList.remove("hidden");
                }
            }

            // Hide computed stats if there is no simulation data
            if (!hasSimulationData)
            {
                const elementsToHide = Helpers.getElementsByDataType("simulationData");
                if (elementsToHide)
                {
                    for (let i = 0; i < elementsToHide.length; i++)
                    {
                        elementsToHide[i].classList.add("hidden");
                    }
                }
            }

            // Hide outsider stats if there is no data
            if (!hasOutsiderData)
            {
                const elementsToHide = Helpers.getElementsByDataType("outsiderLevels");
                if (elementsToHide)
                {
                    for (let i = 0; i < elementsToHide.length; i++)
                    {
                        elementsToHide[i].classList.add("hidden");
                    }
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

            if (statType.indexOf("suggested") === 0)
            {
                const diffStatType = statType.replace("suggested", "diff");
                const ancientStatType = statType.replace("suggested", "ancient");
                const itemStatType = statType.replace("suggested", "item");
                const ancientStatValue = stats[ancientStatType] || 0;
                const itemStatValue = Math.floor(stats[itemStatType]) || 0;

                let diffStatValue = statValue - ancientStatValue;
                if (userSettings.useEffectiveLevelForSuggestions)
                {
                    diffStatValue -= itemStatValue;
                }

                hydrateStat(stats, diffStatType, diffStatValue);
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

    function displayFailure(): void
    {
        // BUGBUG 51: Create Loading and Failure states for ajax loading
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

    function getAncientSouls(): number
    {
        return parseInt(Helpers.getElementsByDataType("totalAncientSouls")[0].textContent);
    }

    const uploadId = Helpers.getElementsByDataType("uploadId")[0].textContent;

    $.ajax({
        url: "/api/uploads/" + uploadId,
    })
        .done(handleSuccess)
        .fail(displayFailure);
}
