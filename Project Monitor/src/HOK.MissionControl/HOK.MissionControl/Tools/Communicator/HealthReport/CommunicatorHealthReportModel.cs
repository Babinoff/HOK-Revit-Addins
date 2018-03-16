﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Families;

namespace HOK.MissionControl.Tools.Communicator.HealthReport
{
    public class CommunicatorHealthReportModel
    {
        public ObservableCollection<HealthReportSummaryViewModel> ProcessData(HealthReportData data, FamilyData familyStats)
        {
            var output = new ObservableCollection<HealthReportSummaryViewModel>();

            var worksetStats = data.itemCount.LastOrDefault();
            if (worksetStats != null)
            {
                var unusedWorksets = 0;
                var workset1 = false;
                var sharedLevels = false;
                var contentOnSingleWorkset = false;
                var worksetCountTotal = worksetStats.worksets.Sum(w => w.count);

                foreach (var w in worksetStats.worksets)
                {
                    if (w.count <= 0) unusedWorksets++;
                    if (w.name == "Workset1") workset1 = true;
                    if (w.name == "Shared Levels and Grids") sharedLevels = true;
                    if ((w.count * 100) / worksetCountTotal >= 50) contentOnSingleWorkset = true;
                }

                var passingChecks = 0;
                if (unusedWorksets == 0) passingChecks += 2;
                if (workset1 && sharedLevels && worksetStats.worksets.Count == 2) passingChecks += 0;
                else passingChecks += 2;
                if (!contentOnSingleWorkset) passingChecks += 2;

                const int maxScore = 6;
                var vm = new HealthReportSummaryViewModel
                {
                    Count = worksetStats.worksets.Count.ToString(),
                    Title = "Worksets:",
                    ToolName = string.Empty,
                    ShowButton = false,
                    Score = passingChecks + "/" + maxScore,
                    FillColor = GetColor(passingChecks, maxScore)
                };
                output.Add(vm);
            }

            var linkStats = data.linkStats.LastOrDefault();
            if (linkStats != null)
            {
                var passingChecks = 0;
                var imports = linkStats.importedDwgFiles.Select(x => !x.isLinked).Count();
                if (imports == 0) passingChecks += 2;
                if (linkStats.unusedLinkedImages == 0) passingChecks += 2;
                else
                {
                    if (linkStats.unusedLinkedImages <= 2) passingChecks += 1;
                }
                if (linkStats.totalImportedStyles <= 25) passingChecks += 2;
                else
                {
                    if (linkStats.totalImportedStyles > 25 && linkStats.totalImportedStyles <= 50) passingChecks += 1;
                }

                const int maxScore = 6;
                var vm = new HealthReportSummaryViewModel
                {
                    Count = linkStats.totalImportedDwg.ToString(),
                    Title = "Links:",
                    ToolName = "Links Manager",
                    ShowButton = true,
                    Score = passingChecks + "/" + maxScore,
                    FillColor = GetColor(passingChecks, maxScore)
                };
                output.Add(vm);
            }

            var viewStats = data.viewStats.LastOrDefault();
            if (viewStats != null)
            {
                var viewsNotOnSheet = viewStats.totalViews - viewStats.viewsOnSheet;
                var schedulesNotOnSheet = viewStats.totalSchedules - viewStats.schedulesOnSheet;

                var viewsNotOnSheetP = Math.Round(
                    (double)((viewsNotOnSheet * 100) / viewStats.totalViews), 0);
                var schedulesNotOnSheetP = Math.Round(
                    (double)((schedulesNotOnSheet * 100) / viewStats.totalSchedules), 0);

                var passingChecks = 0;
                if (viewsNotOnSheetP <= 20) passingChecks += 2;
                else
                {
                    if (viewsNotOnSheetP > 20 && viewsNotOnSheetP <= 40) passingChecks += 1;
                }
                if (schedulesNotOnSheetP <= 20) passingChecks += 2;
                else
                {
                    if (schedulesNotOnSheetP > 20 && schedulesNotOnSheetP <= 40) passingChecks += 1;
                }
                var templateValue = Math.Round(
                    (double)((viewStats.viewsOnSheetWithTemplate * 100) / viewStats.viewsOnSheet), 0);
                if (templateValue >= 80) passingChecks += 2;
                else
                {
                    if (templateValue > 70 && templateValue <= 80) passingChecks += 1;
                }
                if (viewStats.unclippedViews <= 20) passingChecks += 2;

                const int maxScore = 8;
                var vm = new HealthReportSummaryViewModel
                {
                    Count = viewStats.totalViews.ToString(),
                    Title = "Views:",
                    ToolName = string.Empty,
                    ShowButton = false,
                    Score = passingChecks + "/" + maxScore,
                    FillColor = GetColor(passingChecks, maxScore)
                };
                output.Add(vm);
            }

            var stylesStats = data.styleStats.LastOrDefault();
            if (stylesStats != null)
            {
                var overridenDimensions = stylesStats.dimSegmentStats.Count;

                var passingChecks = 0;
                if (overridenDimensions <= 10) passingChecks += 2;
                else if (overridenDimensions > 10 && overridenDimensions <= 20) passingChecks += 1;

                var unusedTextTypes = true;
                var unusedDimensionTypes = true;
                var usesProjectUnits = true;
                var unusedTypes = 0;
                foreach (var ds in stylesStats.dimStats)
                {
                    if (ds.instances == 0)
                    {
                        unusedTypes += 1;
                        unusedDimensionTypes = false;
                    }
                    if (!ds.usesProjectUnits) usesProjectUnits = false;
                }
                foreach (var ts in stylesStats.textStats)
                {
                    if (ts.instances == 0)
                    {
                        unusedTypes += 1;
                        unusedTextTypes = false;
                    }
                }

                if (usesProjectUnits) passingChecks += 2;
                if (unusedDimensionTypes) passingChecks += 2;
                if (unusedTextTypes) passingChecks += 2;

                const int maxScore = 8;
                var vm = new HealthReportSummaryViewModel
                {
                    Count = unusedTypes.ToString(),
                    Title = "Styles:",
                    ToolName = string.Empty,
                    ShowButton = false,
                    Score = passingChecks + "/" + maxScore,
                    FillColor = GetColor(passingChecks, maxScore)
                };
                output.Add(vm);
            }

            var modelStats = data.modelSizes.LastOrDefault();
            if (modelStats != null)
            {
                var vm = new HealthReportSummaryViewModel
                {
                    Count = StringUtilities.BytesToString(modelStats.value),
                    Title = "Model:",
                    ToolName = string.Empty,
                    Score = "0/0",
                    FillColor = Color.FromRgb(119, 119, 119)
                };
                output.Add(vm);
            }

            if (familyStats != null)
            {
                var passingChecks = 0;
                if (familyStats.oversizedFamilies <= 20) passingChecks += 2;
                else
                {
                    if (familyStats.oversizedFamilies > 10 && familyStats.oversizedFamilies < 20) passingChecks += 1;
                }
                var misnamed = familyStats.families.Count(f => f.isFailingChecks && !f.name.Contains("_HOK_I") && !f.name.Contains("_HOK_M"));
                if (misnamed < 10) passingChecks += 2;
                else
                {
                    if (misnamed > 10 && misnamed < 20) passingChecks += 1;
                }
                if (familyStats.unusedFamilies <= 10) passingChecks += 2;
                else
                {
                    if (familyStats.unusedFamilies > 10 && familyStats.unusedFamilies < 20) passingChecks += 1;
                }
                if (familyStats.inPlaceFamilies <= 5) passingChecks += 2;
                else
                {
                    if (familyStats.inPlaceFamilies > 5 && familyStats.inPlaceFamilies < 10) passingChecks += 1;
                }

                const int maxScore = 8;
                var vm = new HealthReportSummaryViewModel
                {
                    Count = familyStats.totalFamilies.ToString(),
                    Title = "Families:",
                    ToolName = string.Empty,
                    ShowButton = false,
                    Score = passingChecks + "/" + maxScore,
                    FillColor = GetColor(passingChecks, maxScore)
                };
                output.Add(vm);
            }

            return output;
        }

        /// <summary>
        /// Returns a Color based on score and max value.
        /// </summary>
        /// <param name="score">Passing Checks score.</param>
        /// <param name="newMax">Max value that can be scored.</param>
        /// <returns></returns>
        private static Color GetColor(int score, int newMax)
        {
            var remapedScore = Math.Round(
                (double)((score * 6) / newMax));
            if (remapedScore <= 2)
            {
                return Color.FromRgb(217, 83, 79);
            }
            if (remapedScore >= 2 && remapedScore <= 4)
            {
                return Color.FromRgb(240, 173, 78);
            }
            return Color.FromRgb(92, 182, 92);
        }
    }
}
