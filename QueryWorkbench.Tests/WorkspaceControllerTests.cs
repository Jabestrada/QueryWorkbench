using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryWorkbench.Tests.Mocks;
using QueryWorkBench.UI;
using QueryWorkbenchUI.Models;
using QueryWorkbenchUI.UserControls;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace QueryWorkbench.Tests {
    [TestClass]
    public class WorkspaceControllerTests {
        [TestMethod]
        public void itShouldHaveZeroWorkspacesOnLoad() {
            var sut = new Main();
            Assert.AreEqual(0, sut.Workspaces.Count);
        }

        [TestMethod]
        public void itShouldCreateNewWorkspaceOnCtrlN() {
            var sut = new Main();

            var model = new Workspace {
                ConnectionString = "qwerty",
                Query = "SELECT * FROM Test",
                Parameters = "@1 = a @2 = b"
            };

            sut.AddWorkspace("test", new QueryWorkspaceView(model));

            Assert.AreEqual(1, sut.Workspaces.Count);

            sut.SendKeys(Keys.Control | Keys.N);

            Assert.IsFalse(sut.Workspaces.First().Model.HasSameValueAs(sut.Workspaces.Last().Model));
        }


        [TestMethod]
        public void itShouldCloneActiveWorkspaceOnCtrlD() {
            var sut = new Main();

            var model = new Workspace {
                ConnectionString = "qwerty",
                Query = "SELECT * FROM Test",
                Parameters = "@1 = a @2 = b"
            };

            sut.AddWorkspace("test", new QueryWorkspaceView(model));

            Assert.AreEqual(1, sut.Workspaces.Count);

            sut.SendKeys(Keys.Control | Keys.D);

            Assert.IsTrue(sut.Workspaces.First().Model.HasSameValueAs(sut.Workspaces.Last().Model));
        }

        [TestMethod]
        public void itShouldApplyFilterOnCtrlF() {
            var sut = new Main();
            var workspace1 = new MockQueryWorkspaceView();
            var workspace2 = new MockQueryWorkspaceView();

            sut.AddWorkspace("test1", workspace1);

            sut.AddWorkspace("test2", workspace2);

            sut.SendKeys(Keys.Control | Keys.F);

            Assert.IsFalse(workspace1.DidApplyFilter);
            Assert.IsTrue(workspace2.DidApplyFilter);
        }

        [TestMethod]
        public void itShouldRunQueryOnCtrlE() {
            var sut = new Main();
            var workspace1 = new MockQueryWorkspaceView(new MockCommandDispatcher());
            var workspace2 = new MockQueryWorkspaceView(new MockCommandDispatcher());
            

            sut.AddWorkspace("test1", workspace1);

            sut.AddWorkspace("test2", workspace2);

            sut.SendKeys(Keys.Control | Keys.E);

            Assert.IsFalse(workspace1.DidRunQuery);
            Assert.IsTrue(workspace2.DidRunQuery);
        }


        [TestMethod]
        public void itShouldForceCloseWorkspaceOnCtrlQ() {
            var sut = new Main();

            var mockWorkspace = new MockQueryWorkspaceView(true);

            sut.AddWorkspace("test", mockWorkspace);

            sut.SendKeys(Keys.Control | Keys.Q);

            Assert.IsTrue(mockWorkspace.WasClosed);
        }



        [DataTestMethod]
        [DataRow(true, false)]
        [DataRow(false, true)]
        public void itShouldLetWorkspaceDecideIfItClosesOrNotWhenNotForcedClose(bool isDirty, bool expectedClosed) {
            var sut = new Main();
            var mockWorkspace = new MockQueryWorkspaceView(isDirty);

            sut.AddWorkspace("test", mockWorkspace);

            sut.SendKeys(Keys.Control | Keys.W);

            Assert.IsTrue(mockWorkspace.WasClosed == expectedClosed);
        }

        [TestMethod]
        public void itShouldShowOpenFileDialogOnCtrlO() {
            var mockOpenDialog = new MockOpenFileDialog();
            var sut = new Main().WithOpenFileDialog(mockOpenDialog);

            sut.SendKeys(Keys.Control | Keys.O);

            Assert.IsTrue(mockOpenDialog.OpenDialogShown);
        }


        [TestMethod]
        public void itShouldCycleWorkspaceTabsForwardOnCtrlT() {
            var sut = new Main();
            var workspace1 = new MockQueryWorkspaceView();
            var workspace2 = new MockQueryWorkspaceView();
            var workspace3 = new MockQueryWorkspaceView();

            sut.AddWorkspace("test1", workspace1);
            sut.AddWorkspace("test2", workspace2);
            sut.AddWorkspace("test3", workspace3);

            sut.SendKeys(Keys.Control | Keys.T);

            Assert.AreEqual(sut.ActiveQueryWorkspace, workspace1);

            sut.SendKeys(Keys.Control | Keys.T);

            Assert.AreEqual(sut.ActiveQueryWorkspace, workspace2);

            sut.SendKeys(Keys.Control | Keys.T);

            Assert.AreEqual(sut.ActiveQueryWorkspace, workspace3);

        }

        [TestMethod]
        public void itShouldCycleWorkspaceTabsBackwardOnCtrlShiftT() {
            var sut = new Main();
            var workspace1 = new MockQueryWorkspaceView();
            var workspace2 = new MockQueryWorkspaceView();
            var workspace3 = new MockQueryWorkspaceView();

            sut.AddWorkspace("test1", workspace1);
            sut.AddWorkspace("test2", workspace2);
            sut.AddWorkspace("test3", workspace3);

            sut.SendKeys(Keys.Control | Keys.Shift | Keys.T);

            Assert.AreEqual(sut.ActiveQueryWorkspace, workspace2);

            sut.SendKeys(Keys.Control | Keys.Shift | Keys.T);

            Assert.AreEqual(sut.ActiveQueryWorkspace, workspace1);

            sut.SendKeys(Keys.Control | Keys.Shift | Keys.T);

            Assert.AreEqual(sut.ActiveQueryWorkspace, workspace3);
        }


        [TestMethod]
        public void itShouldSaveWorkspaceOnCtrlS() {
            var sut = new Main();
            var workspace1 = new MockQueryWorkspaceView();
            var workspace2 = new MockQueryWorkspaceView();
            var workspace3 = new MockQueryWorkspaceView();

            sut.AddWorkspace("test1", workspace1);
            sut.AddWorkspace("test2", workspace2);
            sut.AddWorkspace("test3", workspace3);

            sut.SendKeys(Keys.Control | Keys.S);

            Assert.IsFalse(workspace1.DidSaveWorkspace);
            Assert.IsFalse(workspace2.DidSaveWorkspace);
            Assert.IsTrue(workspace3.DidSaveWorkspace);

        }

        [TestMethod]
        public void itShouldAddNewWorkspaceToMRUOnSave() {

            var sut = new Main().WithAppStateStore(new MockAppStateStore());
            var workspaceFile = "TestFile.qws";
            var workspace1 = new MockQueryWorkspaceView(workspaceFile);

            sut.AddWorkspace("test1", workspace1);

            sut.SendKeys(Keys.Control | Keys.S);

            Assert.IsTrue(workspace1.DidSaveWorkspace);

            Assert.IsTrue(sut.MRUItems[0].Text.Contains(workspaceFile));
        }

        [TestMethod]
        public void itShouldToggleOutputPaneVisibility() {
            var sut = new Main();
            var mockWorkspace = new MockQueryWorkspaceView(new MockCommandDispatcher());

            sut.AddWorkspace("test", mockWorkspace);

            sut.SendKeys(Keys.Control | Keys.E);

            // Output pane visible by default after command execution
            Assert.IsTrue(mockWorkspace.IsOutputPaneVisible);

            // Toggle off
            sut.SendKeys(Keys.Control | Keys.Shift | Keys.O);

            Assert.IsFalse(mockWorkspace.IsOutputPaneVisible);

            // Toggle back on
            sut.SendKeys(Keys.Control | Keys.Shift | Keys.O);
            
            Assert.IsTrue(mockWorkspace.IsOutputPaneVisible);

        }
    }
}
