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
                userNameElements[i].innerText = upload.user.name;
            }
            else
            {
                userNameElements[i].classList.add("text-muted");
                userNameElements[i].innerText = "(Anonymous)";
            }
        }

        const submitTimeElements = Helpers.getElementsByDataType("uploadSubmitTime");
        if (upload.timeSubmitted)
        {
            const timeSubmitted = new Date(upload.timeSubmitted);
            for (let i = 0; i < submitTimeElements.length; i++)
            {
                submitTimeElements[i].innerText = timeSubmitted.toLocaleString();
            }
        }

        const uploadContentElements = Helpers.getElementsByDataType("uploadContent");
        if (upload.uploadContent)
        {
            for (let i = 0; i < uploadContentElements.length; i++)
            {
                uploadContentElements[i].innerText = upload.uploadContent;
            }
        }

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
                        if (!upload.stats.hasOwnProperty(dataType) && cell.innerText === "0")
                        {
                            row.classList.add("hidden");
                        }
                    }
                }
            }

            const hasOutsiderData = upload.stats.hasOwnProperty("outsiderXyliqil");

            // Show the correct suggested ancient level text
            if (!hasOutsiderData)
            {
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

            // If there is data for outsiders, hide computed stats. Else hide outsiders.
            const dataTypeToHide = hasOutsiderData
                ? "simulationData"
                : "outsiderLevels";
            const elementsToHide = Helpers.getElementsByDataType(dataTypeToHide);
            if (elementsToHide)
            {
                for (let i = 0; i < elementsToHide.length; i++)
                {
                    elementsToHide[i].classList.add("hidden");
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

                        displayText = effectiveLevelDisplayText;

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

                hydrateStat(stats, diffStatType, statValue - ancientStatValue - itemStatValue);
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

                statElements[i].innerText = displayText;
            }
        }
    }

    function displayFailure(): void
    {
        // BUGBUG 51: Create Loading and Failure states for ajax loading
    }

    const uploadId = Helpers.getElementsByDataType("uploadId")[0].innerHTML;

    $.ajax({
        url: "/api/uploads/" + uploadId,
    })
        .done(handleSuccess)
        .fail(displayFailure);
}
