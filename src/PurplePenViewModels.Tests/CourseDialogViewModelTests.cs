/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 *
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using PurplePen;
using PurplePen.ViewModels;

namespace PurplePenViewModels.Tests
{
    // ============================================================
    // AllControlsPropertiesDialogViewModel
    // ============================================================

    /// <summary>
    /// Tests for AllControlsPropertiesDialogViewModel — scale list, computed PrintScale
    /// and DescKind round-trips.
    /// </summary>
    [TestFixture]
    public class AllControlsPropertiesDialogViewModelTests
    {
        /// <summary>
        /// Design-time constructor should not throw and should populate defaults.
        /// </summary>
        [Test]
        public void DesignTimeCtor_DoesNotThrow()
        {
            AllControlsPropertiesDialogViewModel vm = new AllControlsPropertiesDialogViewModel();
            Assert.That(vm.PrintScaleText, Is.EqualTo("10000"));
            Assert.That(vm.DescKindIndex, Is.EqualTo(0));
        }

        /// <summary>
        /// Runtime constructor should populate the scale list from the map scale.
        /// </summary>
        [Test]
        public void RuntimeCtor_PopulatesScaleList()
        {
            AllControlsPropertiesDialogViewModel vm = new AllControlsPropertiesDialogViewModel(
                10000f, 15000f, DescriptionKind.Text);
            Assert.That(vm.AvailablePrintScales.Count, Is.GreaterThan(0));
        }

        /// <summary>
        /// Runtime constructor should set PrintScaleText from the print scale argument.
        /// </summary>
        [Test]
        public void RuntimeCtor_SetsPrintScaleText()
        {
            AllControlsPropertiesDialogViewModel vm = new AllControlsPropertiesDialogViewModel(
                10000f, 15000f, DescriptionKind.Text);
            Assert.That(vm.PrintScaleText, Is.EqualTo("15000"));
        }

        /// <summary>
        /// PrintScale computed property should parse PrintScaleText as a float.
        /// </summary>
        [Test]
        public void PrintScale_ParsesText()
        {
            AllControlsPropertiesDialogViewModel vm = new AllControlsPropertiesDialogViewModel(
                10000f, 10000f, DescriptionKind.Symbols);
            vm.PrintScaleText = "7500";
            Assert.That(vm.PrintScale, Is.EqualTo(7500f));
        }

        /// <summary>
        /// PrintScale should return 0 when PrintScaleText is not a number.
        /// </summary>
        [Test]
        public void PrintScale_ReturnsZeroForInvalidText()
        {
            AllControlsPropertiesDialogViewModel vm = new AllControlsPropertiesDialogViewModel(
                10000f, 10000f, DescriptionKind.Symbols);
            vm.PrintScaleText = "not-a-number";
            Assert.That(vm.PrintScale, Is.EqualTo(0f));
        }

        /// <summary>
        /// DescKind round-trip: each enum value maps to the correct index and back.
        /// </summary>
        [TestCase(DescriptionKind.Symbols, 0)]
        [TestCase(DescriptionKind.Text, 1)]
        [TestCase(DescriptionKind.SymbolsAndText, 2)]
        public void DescKind_RoundTrips(DescriptionKind kind, int expectedIndex)
        {
            AllControlsPropertiesDialogViewModel vm = new AllControlsPropertiesDialogViewModel(
                10000f, 10000f, kind);
            Assert.That(vm.DescKindIndex, Is.EqualTo(expectedIndex));
            Assert.That(vm.DescKind, Is.EqualTo(kind));
        }
    }

    // ============================================================
    // CourseLoadItem / CourseLoadDialogViewModel
    // ============================================================

    /// <summary>
    /// Tests for CourseLoadItem — load-text parsing and ToUpdatedInfo round-trip.
    /// </summary>
    [TestFixture]
    public class CourseLoadItemTests
    {
        private static Controller.CourseLoadInfo MakeInfo(string name, int load)
        {
            return new Controller.CourseLoadInfo { courseName = name, load = load };
        }

        /// <summary>
        /// Constructor should display a positive load as a string.
        /// </summary>
        [Test]
        public void Ctor_PositiveLoad_ShowsText()
        {
            CourseLoadItem item = new CourseLoadItem(MakeInfo("Course A", 42));
            Assert.That(item.CourseName, Is.EqualTo("Course A"));
            Assert.That(item.LoadText, Is.EqualTo("42"));
        }

        /// <summary>
        /// Constructor should display an empty string for a non-positive load (unset).
        /// </summary>
        [TestCase(0)]
        [TestCase(-1)]
        public void Ctor_NonPositiveLoad_ShowsEmpty(int load)
        {
            CourseLoadItem item = new CourseLoadItem(MakeInfo("Course A", load));
            Assert.That(item.LoadText, Is.EqualTo(""));
        }

        /// <summary>
        /// ToUpdatedInfo should store the parsed load for valid positive integer text.
        /// </summary>
        [Test]
        public void ToUpdatedInfo_ValidLoad_Stored()
        {
            CourseLoadItem item = new CourseLoadItem(MakeInfo("Course A", 0));
            item.LoadText = "100";
            Controller.CourseLoadInfo result = item.ToUpdatedInfo();
            Assert.That(result.load, Is.EqualTo(100));
        }

        /// <summary>
        /// ToUpdatedInfo should store -1 when the load text is empty (unset).
        /// </summary>
        [Test]
        public void ToUpdatedInfo_EmptyText_StoresNegativeOne()
        {
            CourseLoadItem item = new CourseLoadItem(MakeInfo("Course A", 50));
            item.LoadText = "";
            Controller.CourseLoadInfo result = item.ToUpdatedInfo();
            Assert.That(result.load, Is.EqualTo(-1));
        }

        /// <summary>
        /// ToUpdatedInfo should store -1 for non-numeric text.
        /// </summary>
        [Test]
        public void ToUpdatedInfo_InvalidText_StoresNegativeOne()
        {
            CourseLoadItem item = new CourseLoadItem(MakeInfo("Course A", 50));
            item.LoadText = "abc";
            Controller.CourseLoadInfo result = item.ToUpdatedInfo();
            Assert.That(result.load, Is.EqualTo(-1));
        }

        /// <summary>
        /// ToUpdatedInfo should store -1 for zero (not a valid load).
        /// </summary>
        [Test]
        public void ToUpdatedInfo_Zero_StoresNegativeOne()
        {
            CourseLoadItem item = new CourseLoadItem(MakeInfo("Course A", 50));
            item.LoadText = "0";
            Controller.CourseLoadInfo result = item.ToUpdatedInfo();
            Assert.That(result.load, Is.EqualTo(-1));
        }

        /// <summary>
        /// ToUpdatedInfo should preserve the course name.
        /// </summary>
        [Test]
        public void ToUpdatedInfo_PreservesCourseName()
        {
            CourseLoadItem item = new CourseLoadItem(MakeInfo("My Course", 10));
            item.LoadText = "20";
            Controller.CourseLoadInfo result = item.ToUpdatedInfo();
            Assert.That(result.courseName, Is.EqualTo("My Course"));
        }
    }

    /// <summary>
    /// Tests for CourseLoadDialogViewModel — design-time constructor and GetCourseLoads.
    /// </summary>
    [TestFixture]
    public class CourseLoadDialogViewModelTests
    {
        /// <summary>
        /// Design-time constructor should pre-populate sample data for the designer.
        /// </summary>
        [Test]
        public void DesignTimeCtor_PrePopulatesSampleData()
        {
            CourseLoadDialogViewModel vm = new CourseLoadDialogViewModel();
            Assert.That(vm.CourseLoads.Count, Is.GreaterThan(0));
        }

        /// <summary>
        /// GetCourseLoads should return the same number of entries that were loaded.
        /// </summary>
        [Test]
        public void GetCourseLoads_ReturnsAllEntries()
        {
            CourseLoadDialogViewModel vm = new CourseLoadDialogViewModel();
            vm.CourseLoads.Clear(); // clear design-time sample data
            vm.CourseLoads.Add(new CourseLoadItem(new Controller.CourseLoadInfo { courseName = "A", load = 10 }));
            vm.CourseLoads.Add(new CourseLoadItem(new Controller.CourseLoadInfo { courseName = "B", load = 20 }));
            Controller.CourseLoadInfo[] result = vm.GetCourseLoads();
            Assert.That(result.Length, Is.EqualTo(2));
        }
    }

    // ============================================================
    // CourseOrderDialogViewModel
    // ============================================================

    /// <summary>
    /// Tests for CourseOrderDialogViewModel — move commands and CanMove guards.
    /// </summary>
    [TestFixture]
    public class CourseOrderDialogViewModelTests
    {
        private static CourseOrderItem MakeItem(string name) =>
            new CourseOrderItem(new Controller.CourseOrderInfo { courseName = name, sortOrder = 0 });

        private CourseOrderDialogViewModel BuildVm(params string[] names)
        {
            CourseOrderDialogViewModel vm = new CourseOrderDialogViewModel();
            vm.Courses.Clear(); // design-time ctor pre-populates sample data; clear it for tests
            foreach (string name in names)
                vm.Courses.Add(MakeItem(name));
            return vm;
        }

        /// <summary>
        /// Design-time constructor should pre-populate sample courses for the designer.
        /// </summary>
        [Test]
        public void DesignTimeCtor_PrePopulatesSampleData()
        {
            CourseOrderDialogViewModel vm = new CourseOrderDialogViewModel();
            Assert.That(vm.Courses.Count, Is.GreaterThan(0));
        }

        /// <summary>
        /// CanMoveUp should be false with no selection.
        /// </summary>
        [Test]
        public void CanMoveUp_FalseWithNoSelection()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B");
            vm.SelectedIndex = -1;
            Assert.That(vm.CanMoveUp, Is.False);
        }

        /// <summary>
        /// CanMoveUp should be false at the first item.
        /// </summary>
        [Test]
        public void CanMoveUp_FalseAtFirst()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B");
            vm.SelectedIndex = 0;
            Assert.That(vm.CanMoveUp, Is.False);
        }

        /// <summary>
        /// CanMoveUp should be true when selection is not at the top.
        /// </summary>
        [Test]
        public void CanMoveUp_TrueWhenNotFirst()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B");
            vm.SelectedIndex = 1;
            Assert.That(vm.CanMoveUp, Is.True);
        }

        /// <summary>
        /// CanMoveDown should be false with no selection.
        /// </summary>
        [Test]
        public void CanMoveDown_FalseWithNoSelection()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B");
            vm.SelectedIndex = -1;
            Assert.That(vm.CanMoveDown, Is.False);
        }

        /// <summary>
        /// CanMoveDown should be false at the last item.
        /// </summary>
        [Test]
        public void CanMoveDown_FalseAtLast()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B");
            vm.SelectedIndex = 1;
            Assert.That(vm.CanMoveDown, Is.False);
        }

        /// <summary>
        /// CanMoveDown should be true when selection is not at the bottom.
        /// </summary>
        [Test]
        public void CanMoveDown_TrueWhenNotLast()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B");
            vm.SelectedIndex = 0;
            Assert.That(vm.CanMoveDown, Is.True);
        }

        /// <summary>
        /// MoveUp command should swap the selected item with the one above it and
        /// keep SelectedIndex pointing at the moved item.
        /// </summary>
        [Test]
        public void MoveUp_SwapsItemAndAdjustsIndex()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B", "C");
            vm.SelectedIndex = 1; // "B" selected

            vm.MoveUpCommand.Execute(null);

            Assert.That(vm.Courses[0].CourseName, Is.EqualTo("B"));
            Assert.That(vm.Courses[1].CourseName, Is.EqualTo("A"));
            Assert.That(vm.Courses[2].CourseName, Is.EqualTo("C"));
            Assert.That(vm.SelectedIndex, Is.EqualTo(0));
        }

        /// <summary>
        /// MoveDown command should swap the selected item with the one below it and
        /// keep SelectedIndex pointing at the moved item.
        /// </summary>
        [Test]
        public void MoveDown_SwapsItemAndAdjustsIndex()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B", "C");
            vm.SelectedIndex = 1; // "B" selected

            vm.MoveDownCommand.Execute(null);

            Assert.That(vm.Courses[0].CourseName, Is.EqualTo("A"));
            Assert.That(vm.Courses[1].CourseName, Is.EqualTo("C"));
            Assert.That(vm.Courses[2].CourseName, Is.EqualTo("B"));
            Assert.That(vm.SelectedIndex, Is.EqualTo(2));
        }

        /// <summary>
        /// MoveUp command should do nothing when CanMoveUp is false (no selection).
        /// </summary>
        [Test]
        public void MoveUp_DoesNothingWhenCannotMove()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B");
            vm.SelectedIndex = 0;
            vm.MoveUpCommand.Execute(null);
            Assert.That(vm.Courses[0].CourseName, Is.EqualTo("A"));
            Assert.That(vm.Courses[1].CourseName, Is.EqualTo("B"));
        }

        /// <summary>
        /// MoveDown command should do nothing when CanMoveDown is false.
        /// </summary>
        [Test]
        public void MoveDown_DoesNothingWhenCannotMove()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B");
            vm.SelectedIndex = 1;
            vm.MoveDownCommand.Execute(null);
            Assert.That(vm.Courses[0].CourseName, Is.EqualTo("A"));
            Assert.That(vm.Courses[1].CourseName, Is.EqualTo("B"));
        }

        /// <summary>
        /// GetCourseOrders should assign sortOrder 1..N in list order.
        /// </summary>
        [Test]
        public void GetCourseOrders_AssignsSortOrderInListOrder()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B", "C");
            Controller.CourseOrderInfo[] result = vm.GetCourseOrders();
            Assert.That(result[0].sortOrder, Is.EqualTo(1));
            Assert.That(result[1].sortOrder, Is.EqualTo(2));
            Assert.That(result[2].sortOrder, Is.EqualTo(3));
        }

        /// <summary>
        /// After MoveUp, GetCourseOrders should assign sort orders in the new list order.
        /// </summary>
        [Test]
        public void GetCourseOrders_ReflectsMoveUpReorder()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B", "C");
            vm.SelectedIndex = 2; // "C"
            vm.MoveUpCommand.Execute(null);
            vm.MoveUpCommand.Execute(null); // "C" is now first

            Controller.CourseOrderInfo[] result = vm.GetCourseOrders();
            Assert.That(result[0].courseName, Is.EqualTo("C"));
            Assert.That(result[0].sortOrder, Is.EqualTo(1));
        }

        /// <summary>
        /// SelectedIndex change should raise PropertyChanged for CanMoveUp and CanMoveDown.
        /// </summary>
        [Test]
        public void SelectedIndex_NotifiesCanMoveProperties()
        {
            CourseOrderDialogViewModel vm = BuildVm("A", "B");
            List<string?> changed = new List<string?>();
            vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);

            vm.SelectedIndex = 1;

            Assert.That(changed, Does.Contain("CanMoveUp"));
            Assert.That(changed, Does.Contain("CanMoveDown"));
        }
    }

    // ============================================================
    // LegAssignmentsDialogViewModel
    // ============================================================

    /// <summary>
    /// Tests for LegAssignmentsDialogViewModel — building from codes, existing assignments,
    /// and round-tripping through GetFixedBranchAssignments.
    /// </summary>
    [TestFixture]
    public class LegAssignmentsDialogViewModelTests
    {
        private static List<char[]> TwoGroups() =>
            new List<char[]> { new[] { 'A', 'B' }, new[] { 'C', 'D' } };

        /// <summary>
        /// Design-time constructor should pre-populate sample data for the designer.
        /// </summary>
        [Test]
        public void DesignTimeCtor_PrePopulatesSampleData()
        {
            LegAssignmentsDialogViewModel vm = new LegAssignmentsDialogViewModel();
            Assert.That(vm.Items.Count, Is.GreaterThan(0));
        }

        /// <summary>
        /// Runtime constructor should create one item per code character.
        /// </summary>
        [Test]
        public void RuntimeCtor_CreatesOneItemPerCode()
        {
            LegAssignmentsDialogViewModel vm = new LegAssignmentsDialogViewModel(
                TwoGroups(), new FixedBranchAssignments());
            Assert.That(vm.Items.Count, Is.EqualTo(4));
        }

        /// <summary>
        /// Items should have the correct branch code letters.
        /// </summary>
        [Test]
        public void RuntimeCtor_SetsCorrectBranchCodes()
        {
            LegAssignmentsDialogViewModel vm = new LegAssignmentsDialogViewModel(
                TwoGroups(), new FixedBranchAssignments());
            string[] codes = vm.Items.Select(i => i.BranchCode).ToArray();
            Assert.That(codes, Is.EqualTo(new[] { "A", "B", "C", "D" }));
        }

        /// <summary>
        /// Alternating group flag should toggle between groups.
        /// </summary>
        [Test]
        public void RuntimeCtor_AlternatesGroupFlag()
        {
            LegAssignmentsDialogViewModel vm = new LegAssignmentsDialogViewModel(
                TwoGroups(), new FixedBranchAssignments());
            // Group 1 (A, B) → IsAlternateGroup = false; Group 2 (C, D) → true
            Assert.That(vm.Items[0].IsAlternateGroup, Is.False);
            Assert.That(vm.Items[1].IsAlternateGroup, Is.False);
            Assert.That(vm.Items[2].IsAlternateGroup, Is.True);
            Assert.That(vm.Items[3].IsAlternateGroup, Is.True);
        }

        /// <summary>
        /// Existing fixed assignments should be pre-populated as 1-based leg text.
        /// </summary>
        [Test]
        public void RuntimeCtor_PrePopulatesExistingAssignments()
        {
            FixedBranchAssignments existing = new FixedBranchAssignments();
            existing.AddBranchAssignment('A', 0); // leg 1 (0-based)
            existing.AddBranchAssignment('A', 2); // leg 3

            LegAssignmentsDialogViewModel vm = new LegAssignmentsDialogViewModel(
                TwoGroups(), existing);

            LegAssignmentItem itemA = vm.Items.First(i => i.BranchCode == "A");
            // Should show 1-based: "1" and "3"
            Assert.That(itemA.LegsText, Does.Contain("1"));
            Assert.That(itemA.LegsText, Does.Contain("3"));
        }

        /// <summary>
        /// Branches without fixed assignments should have empty LegsText.
        /// </summary>
        [Test]
        public void RuntimeCtor_UnassignedBranchHasEmptyText()
        {
            LegAssignmentsDialogViewModel vm = new LegAssignmentsDialogViewModel(
                TwoGroups(), new FixedBranchAssignments());
            Assert.That(vm.Items.All(i => i.LegsText == ""), Is.True);
        }

        /// <summary>
        /// GetFixedBranchAssignments should return empty assignments when all LegsText is empty.
        /// </summary>
        [Test]
        public void GetFixedBranchAssignments_EmptyText_ReturnsEmpty()
        {
            LegAssignmentsDialogViewModel vm = new LegAssignmentsDialogViewModel(
                TwoGroups(), new FixedBranchAssignments());
            FixedBranchAssignments result = vm.GetFixedBranchAssignments();
            Assert.That(result.BranchIsFixed('A'), Is.False);
        }

        /// <summary>
        /// GetFixedBranchAssignments should parse comma-separated 1-based leg numbers
        /// and store them as 0-based in the model.
        /// </summary>
        [Test]
        public void GetFixedBranchAssignments_ParsesLegsText()
        {
            LegAssignmentsDialogViewModel vm = new LegAssignmentsDialogViewModel(
                TwoGroups(), new FixedBranchAssignments());
            vm.Items.First(i => i.BranchCode == "B").LegsText = "2, 4";

            FixedBranchAssignments result = vm.GetFixedBranchAssignments();

            Assert.That(result.BranchIsFixed('B'), Is.True);
            int[] legs = result.FixedLegsForBranch('B').ToArray();
            Assert.That(legs, Does.Contain(1)); // 0-based for leg 2
            Assert.That(legs, Does.Contain(3)); // 0-based for leg 4
        }

        /// <summary>
        /// Round-trip: assign legs, call GetFixedBranchAssignments, rebuild ViewModel,
        /// verify LegsText is re-populated correctly.
        /// </summary>
        [Test]
        public void RoundTrip_AssignThenRebuild_PreservesLegs()
        {
            LegAssignmentsDialogViewModel vm1 = new LegAssignmentsDialogViewModel(
                TwoGroups(), new FixedBranchAssignments());
            vm1.Items.First(i => i.BranchCode == "C").LegsText = "1, 3";

            FixedBranchAssignments assignments = vm1.GetFixedBranchAssignments();

            LegAssignmentsDialogViewModel vm2 = new LegAssignmentsDialogViewModel(
                TwoGroups(), assignments);

            LegAssignmentItem itemC = vm2.Items.First(i => i.BranchCode == "C");
            Assert.That(itemC.LegsText, Does.Contain("1"));
            Assert.That(itemC.LegsText, Does.Contain("3"));
        }
    }

    // ============================================================
    // TeamVariationsDialogViewModel
    // ============================================================

    /// <summary>
    /// Tests for TeamVariationsDialogViewModel — defaults, property bindings,
    /// and RelaySettings assembly.
    /// </summary>
    [TestFixture]
    public class TeamVariationsDialogViewModelTests
    {
        /// <summary>
        /// Design-time constructor should set sensible defaults.
        /// </summary>
        [Test]
        public void DesignTimeCtor_SensibleDefaults()
        {
            TeamVariationsDialogViewModel vm = new TeamVariationsDialogViewModel();
            Assert.That(vm.NumberOfLegs, Is.EqualTo(1));
            Assert.That(vm.FirstTeamNumber, Is.EqualTo(1));
            Assert.That(vm.NumberOfTeams, Is.EqualTo(0));
            Assert.That(vm.HideVariationsOnMap, Is.False);
            Assert.That(vm.StatusText, Is.EqualTo(""));
        }

        /// <summary>
        /// Runtime constructor should pre-populate from a RelaySettings object.
        /// </summary>
        [Test]
        public void RuntimeCtor_PrePopulatesFromRelaySettings()
        {
            FixedBranchAssignments assignments = new FixedBranchAssignments();
            RelaySettings settings = new RelaySettings(5, 30, 4, assignments);

            TeamVariationsDialogViewModel vm = new TeamVariationsDialogViewModel(settings, true);

            Assert.That(vm.FirstTeamNumber, Is.EqualTo(5));
            Assert.That(vm.NumberOfTeams, Is.EqualTo(30));
            Assert.That(vm.NumberOfLegs, Is.EqualTo(4));
            Assert.That(vm.HideVariationsOnMap, Is.True);
        }

        /// <summary>
        /// RelaySettings property should reflect current NumberOfTeams, NumberOfLegs, FirstTeamNumber.
        /// </summary>
        [Test]
        public void RelaySettings_ReflectsCurrentValues()
        {
            TeamVariationsDialogViewModel vm = new TeamVariationsDialogViewModel();
            vm.NumberOfTeams = 10;
            vm.NumberOfLegs = 3;
            vm.FirstTeamNumber = 1;

            RelaySettings settings = vm.RelaySettings;

            Assert.That(settings.relayTeams, Is.EqualTo(10));
            Assert.That(settings.relayLegs, Is.EqualTo(3));
            Assert.That(settings.firstTeamNumber, Is.EqualTo(1));
        }

        /// <summary>
        /// RelaySettings should update when NumberOfTeams changes.
        /// </summary>
        [Test]
        public void RelaySettings_UpdatesWhenNumberOfTeamsChanges()
        {
            TeamVariationsDialogViewModel vm = new TeamVariationsDialogViewModel();
            vm.NumberOfTeams = 5;
            Assert.That(vm.RelaySettings.relayTeams, Is.EqualTo(5));

            vm.NumberOfTeams = 20;
            Assert.That(vm.RelaySettings.relayTeams, Is.EqualTo(20));
        }

        /// <summary>
        /// Null relayBranchAssignments in RelaySettings constructor should default to
        /// a fresh FixedBranchAssignments (no crash).
        /// </summary>
        [Test]
        public void RuntimeCtor_NullBranchAssignments_DefaultsToEmpty()
        {
            RelaySettings settings = new RelaySettings(1, 10, 2, null);
            TeamVariationsDialogViewModel vm = new TeamVariationsDialogViewModel(settings, false);
            Assert.That(vm.FixedBranchAssignments, Is.Not.Null);
        }

        /// <summary>
        /// Setting StatusText should raise PropertyChanged.
        /// </summary>
        [Test]
        public void StatusText_RaisesPropertyChanged()
        {
            TeamVariationsDialogViewModel vm = new TeamVariationsDialogViewModel();
            List<string?> changed = new List<string?>();
            vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);

            vm.StatusText = "Report opened.";

            Assert.That(changed, Does.Contain("StatusText"));
        }

        /// <summary>
        /// RelaySettings should include FixedBranchAssignments when one is assigned.
        /// </summary>
        [Test]
        public void RelaySettings_IncludesFixedBranchAssignments()
        {
            TeamVariationsDialogViewModel vm = new TeamVariationsDialogViewModel();
            FixedBranchAssignments fa = new FixedBranchAssignments();
            fa.AddBranchAssignment('A', 0);
            vm.FixedBranchAssignments = fa;

            RelaySettings settings = vm.RelaySettings;

            Assert.That(settings.relayBranchAssignments, Is.Not.Null);
            Assert.That(settings.relayBranchAssignments!.BranchIsFixed('A'), Is.True);
        }
    }
}
