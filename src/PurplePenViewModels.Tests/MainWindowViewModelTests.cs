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

using System.ComponentModel;
using NUnit.Framework;
using PurplePen.ViewModels;

namespace PurplePenViewModels.Tests
{
    /// <summary>
    /// Tests for MainWindowViewModel, verifying that the Counter property
    /// and the Increment/Decrement commands work correctly.
    /// </summary>
    [TestFixture]
    public class MainWindowViewModelTests
    {
        private MainWindowViewModel viewModel = null!;

        [SetUp]
        public void Initialize()
        {
            viewModel = new MainWindowViewModel();
        }

        /// <summary>
        /// Counter should start at zero.
        /// </summary>
        [Test]
        public void Counter_InitialValue_IsZero()
        {
            Assert.AreEqual(0, viewModel.Counter);
        }

        /// <summary>
        /// IncrementCounterCommand should increase Counter by 1.
        /// </summary>
        [Test]
        public void IncrementCounterCommand_IncreasesCounterByOne()
        {
            viewModel.IncrementCounterCommand.Execute(null);

            Assert.AreEqual(1, viewModel.Counter);
        }

        /// <summary>
        /// DecrementCounterCommand should decrease Counter by 1.
        /// </summary>
        [Test]
        public void DecrementCounterCommand_DecreasesCounterByOne()
        {
            viewModel.DecrementCounterCommand.Execute(null);

            Assert.AreEqual(-1, viewModel.Counter);
        }

        /// <summary>
        /// Multiple increments should accumulate correctly.
        /// </summary>
        [Test]
        public void IncrementCounterCommand_MultipleTimes_Accumulates()
        {
            viewModel.IncrementCounterCommand.Execute(null);
            viewModel.IncrementCounterCommand.Execute(null);
            viewModel.IncrementCounterCommand.Execute(null);

            Assert.AreEqual(3, viewModel.Counter);
        }

        /// <summary>
        /// Increment and decrement together should produce the correct net result.
        /// </summary>
        [Test]
        public void IncrementAndDecrement_ProducesCorrectResult()
        {
            viewModel.IncrementCounterCommand.Execute(null);
            viewModel.IncrementCounterCommand.Execute(null);
            viewModel.DecrementCounterCommand.Execute(null);

            Assert.AreEqual(1, viewModel.Counter);
        }

        /// <summary>
        /// Setting Counter should raise PropertyChanged so the UI updates.
        /// This verifies that the [ObservableProperty] source generator is
        /// producing the correct notification code.
        /// </summary>
        [Test]
        public void IncrementCounterCommand_RaisesPropertyChanged()
        {
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (sender, args) => {
                changedPropertyName = args.PropertyName;
            };

            viewModel.IncrementCounterCommand.Execute(null);

            Assert.AreEqual("Counter", changedPropertyName);
        }

        /// <summary>
        /// Both commands should always be executable (CanExecute == true).
        /// </summary>
        [Test]
        public void Commands_CanAlwaysExecute()
        {
            Assert.IsTrue(viewModel.IncrementCounterCommand.CanExecute(null));
            Assert.IsTrue(viewModel.DecrementCounterCommand.CanExecute(null));
        }
    }
}
