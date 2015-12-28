﻿// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System.Xml;
using NUnit.UiKit.Controls;
using NSubstitute;

namespace NUnit.Gui.Presenters
{
    using Framework;
    using Views;
    using Model;

    public class ProgressBarPresenterTests
    {
        private IProgressBarView _view;
        private ITestModel _model;
        private ProgressBarPresenter _presenter;

        [SetUp]
        public void CreatePresenter()
        {
            _view = Substitute.For<IProgressBarView>();
            _model = Substitute.For<ITestModel>();

            _presenter = new ProgressBarPresenter(_view, _model);
        }

        [TearDown]
        public void RemovePresenter()
        {
            //if (_presenter != null)
            //    _presenter.Dispose();

            _presenter = null;
        }

        //[Test]
        //public void WhenProjectIsUnloaded_ProgressBar_IsInitialized()
        //{
        //    _settings.IsProjectLoaded.Returns(false);
        //    _settings.IsTestLoaded.Returns(false);
        //    _settings.IsTestRunning.Returns(false);
        //    _settings.ProjectUnloaded += Raise.Event<TestEventHandler>(new TestEventArgs(TestAction.ProjectUnloaded, "Dummy"));

        //    _view.Received().Initialize(100);
        //}

        [Test]
        public void WhenTestsAreUnloaded_ProgressBar_IsInitialized()
        {
            _model.HasTests.Returns(false);
            _model.IsTestRunning.Returns(false);
            _model.TestUnloaded += Raise.Event<TestEventHandler>(new TestEventArgs(TestAction.TestUnloaded));

            _view.Received().Initialize(100);
        }

        [Test]
        public void WhenTestRunBegins_ProgressBar_IsInitialized()
        {
            _model.HasTests.Returns(true);
            _model.IsTestRunning.Returns(true);
            _model.RunStarting += Raise.Event<TestEventHandler>(new TestEventArgs(TestAction.RunStarting, 1234));

            _view.Received().Initialize(1234);
        }

        [Test]
        public void WhenTestCaseCompletes_ProgressIsIncremented()
        {
            int priorValue = _view.Progress;
            var result = new TestNode(XmlHelper.CreateXmlNode("<test-case id='1'/>"));

            _model.TestFinished += Raise.Event<TestEventHandler>(new TestEventArgs(TestAction.TestFinished, result));

            Assert.That(_view.Progress, Is.EqualTo(priorValue + 1));
        }

        [Test]
        public void WhenTestSuiteCompletes_ProgressIsNotIncremented()
        {
            int priorValue = _view.Progress;
            var result = new TestNode(XmlHelper.CreateXmlNode("<test-suite id='1'/>"));

            _model.SuiteFinished += Raise.Event<TestEventHandler>(new TestEventArgs(TestAction.SuiteFinished, result));

            Assert.That(_view.Progress, Is.EqualTo(priorValue));
        }

        //[Test]
        //public void WhenTestSuiteCompletes_ResultIsPostedToView()
        //{
        //    int priorValue = _view.Progress;
        //    var suiteResult = new ResultNode(XmlHelper.CreateTopLevelElement("test-suite"));

        //    _settings.TestFinished += Raise.Event<TestEventHandler>(new TestEventArgs(TestAction.TestFinished, suiteResult));

        //    Assert.That(_view.Progress, Is.EqualTo(priorValue));
        //}
        
        static object[] statusTestCases = new object[] {
            new object[] { TestProgressBarStatus.Success, ResultState.Failure, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Warning, ResultState.Failure, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Failure, ResultState.Failure, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Success, ResultState.Error, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Warning, ResultState.Error, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Failure, ResultState.Error, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Success, ResultState.NotRunnable, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Warning, ResultState.NotRunnable, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Failure, ResultState.NotRunnable, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Success, ResultState.Ignored, TestProgressBarStatus.Warning },
            new object[] { TestProgressBarStatus.Warning, ResultState.Ignored, TestProgressBarStatus.Warning },
            new object[] { TestProgressBarStatus.Failure, ResultState.Ignored, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Success, ResultState.Inconclusive, TestProgressBarStatus.Success },
            new object[] { TestProgressBarStatus.Warning, ResultState.Inconclusive, TestProgressBarStatus.Warning },
            new object[] { TestProgressBarStatus.Failure, ResultState.Inconclusive, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Success, ResultState.Skipped, TestProgressBarStatus.Success },
            new object[] { TestProgressBarStatus.Warning, ResultState.Skipped, TestProgressBarStatus.Warning },
            new object[] { TestProgressBarStatus.Failure, ResultState.Skipped, TestProgressBarStatus.Failure },
            new object[] { TestProgressBarStatus.Success, ResultState.Success, TestProgressBarStatus.Success },
            new object[] { TestProgressBarStatus.Warning, ResultState.Success, TestProgressBarStatus.Warning },
            new object[] { TestProgressBarStatus.Failure, ResultState.Success, TestProgressBarStatus.Failure }
        };

        [TestCaseSource("statusTestCases")]
        public void BarShowsProperStatus(TestProgressBarStatus priorStatus, ResultState resultState, TestProgressBarStatus expectedStatus)
        {
            _view.Status = priorStatus;

            var doc = new XmlDocument();
            if (resultState.Label == string.Empty)
                doc.LoadXml(string.Format("<test-case id='1' result='{0}'/>", resultState.Status));
            else
                doc.LoadXml(string.Format("<test-case id='1' result='{0}' label='{1}'/>", resultState.Status, resultState.Label));
            var result = new TestNode(doc.FirstChild);

            _model.HasTests.Returns(true);
            _model.Tests.Returns(result);
            _model.TestLoaded += Raise.Event<TestEventHandler>(new TestEventArgs(TestAction.TestLoaded, result));
            _model.TestFinished += Raise.Event<TestEventHandler>(new TestEventArgs(TestAction.TestFinished, result));

            Assert.That(_view.Status, Is.EqualTo(expectedStatus));
        }
    }
}