﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TestME
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            lblUserMessage.Text += Globals.logUser.user + " !";
            tabUser.SelectTab(1);
            autocompleteMenu1.Items = Globals.colTags.ToArray();

            Functionality.RefreshMyQuestions();

            pusername.Text = Globals.logUser.user;
            pemail.Text = Globals.logUser.email;
            pdatabase.Text = Utilities.FindControl(Globals.formStart, "txtDatabase").Text;
            Utilities.runInThread(() => 
            {
                Utilities.InvokeMe(pnumQ, () => 
                    {
                        pnumQ.Text = Utilities.AsyncDB().single("SELECT COUNT(*) FROM questions WHERE uid="+Globals.logUser.id);
                    });
            }).Start();
            
        }

        private void dgvMyQ_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            Utilities.rightClickSelect(dgvMyQ, e);
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Question TempQuest = new Question();
            TempQuest.id = int.Parse(dgvMyQ.SelectedRows[0].Cells[1].Value.ToString());
            TempQuest.question = dgvMyQ.SelectedRows[0].Cells[2].Value.ToString();
            TempQuest.anwsers = JsonConvert.DeserializeObject<List<Answer>>(dgvMyQ.SelectedRows[0].Cells[3].Value.ToString());
            TempQuest.dlevel = int.Parse(dgvMyQ.SelectedRows[0].Cells[4].Value.ToString());
            TempQuest.prive = Boolean.Parse(dgvMyQ.SelectedRows[0].Cells[5].Value.ToString());

            new frmAnswers(TempQuest).Show();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Question TempQuest = new Question();
            TempQuest.id = int.Parse(dgvMyQ.SelectedRows[0].Cells[1].Value.ToString());
            TempQuest.question = dgvMyQ.SelectedRows[0].Cells[2].Value.ToString();
            TempQuest.anwsers = JsonConvert.DeserializeObject<List<Answer>>(dgvMyQ.SelectedRows[0].Cells[3].Value.ToString());
            TempQuest.dlevel = int.Parse(dgvMyQ.SelectedRows[0].Cells[4].Value.ToString());
            TempQuest.prive = Boolean.Parse(dgvMyQ.SelectedRows[0].Cells[5].Value.ToString());

            new frmEdit(TempQuest).Show();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var msgbResult = MessageBox.Show("Are you sure that you want to\nPermanatly delete the selected Question ?", "Delete Question",MessageBoxButtons.YesNo,MessageBoxIcon.Warning);
            if (msgbResult == DialogResult.Yes)
            {
                int sid = int.Parse(dgvMyQ.SelectedRows[0].Cells[1].Value.ToString());
                Utilities.runInThread(() =>
                {
                    Utilities.AsyncDB().nQuery("DELETE FROM questions WHERE id = " + sid);
                    Utilities.AsyncDB().nQuery("DELETE FROM tags WHERE qid = " + sid);
                }).Start();
                dgvMyQ.Rows.Remove(dgvMyQ.SelectedRows[0]);
                Utilities.notifyThem(ntfMyQ, "Successfully Deleted !", NotificationBox.Type.Success);
            }
        }

        private void txtAddTags_TextChanged(object sender, EventArgs e)
        {
            Utilities.txtBoxReplaceSpaceNewLine(txtAddTags);
        }

        private void txtAddQ_TextChanged(object sender, EventArgs e)
        {
            Utilities.txtBoxReplaceNewLine(txtAddQ);
        }

        private void txtAnswer_TextChanged(object sender, EventArgs e)
        {
            Utilities.txtBoxReplaceNewLine(txtAnswer);
        }

        private void txtTags_TextChanged(object sender, EventArgs e)
        {
            Utilities.txtBoxReplaceSpaceNewLine(txtTags);
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            btnDeleteSelected.Focus();
            string idsForDelete = "";
            List<int> cids = new List<int>();
            for (int i = 0; i < dgvMyQ.Rows.Count; i++)
            {
                if (bool.Parse(dgvMyQ.Rows[i].Cells[0].Value.ToString()))
                {
                    cids.Add(i);
                    idsForDelete += dgvMyQ.Rows[i].Cells[1].Value.ToString() + ",";
                }
            }
            if (cids.Count > 0)
            {
                var msgbResult = MessageBox.Show("Are you sure that you want to\nPermanatly delete the selected Questions ?", "Delete Question", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (msgbResult == DialogResult.Yes)
                {
                    for (int k = (cids.Count-1); k >= 0; k--)
                    {
                        dgvMyQ.Rows.RemoveAt(int.Parse(cids[k].ToString()));
                    }
                    idsForDelete = idsForDelete.TrimEnd(',');
                    Utilities.runInThread(() =>
                    {
                        Utilities.AsyncDB().nQuery("DELETE FROM questions WHERE id IN (" + idsForDelete + ")");
                        Utilities.AsyncDB().nQuery("DELETE FROM tags WHERE qid IN (" + idsForDelete + ")");
                        Utilities.notifyThem(ntfMyQ, "Successfully Deleted " + cids.Count + " Questions !", NotificationBox.Type.Success);
                    }).Start();
                }
            }
            else
            {
                Utilities.notifyThem(ntfMyQ, "You didn't select any questions.", NotificationBox.Type.Warning);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAddQ.Text.Trim()) == true || string.IsNullOrEmpty(txtAddTags.Text.Trim()) == true || (dgvAnswerlist.Rows.Count < 1))
            {
                Utilities.notifyThem(ntfAdd, "You must fill some info about your question first.", NotificationBox.Type.Error);
            }
            else
            {
                List<Answer> Answers = new List<Answer>();
                for(int i=0; i < dgvAnswerlist.Rows.Count; i++)
                {
                    Answers.Add(new Answer(dgvAnswerlist.Rows[i].Cells[0].Value.ToString().TrimEnd().TrimStart(), bool.Parse(dgvAnswerlist.Rows[i].Cells[1].Value.ToString())));
                }
                Utilities.runInThread(() =>
                {
                    DB TempDB = Utilities.AsyncDB(true);
                    TempDB.bind(new string[] {"Question",txtAddQ.Text.TrimEnd().TrimStart(), "Answers", JsonConvert.SerializeObject(Answers).ToString(), "Dlevel", difficultyLvl.Value.ToString(), "Prive", (switchPrivate.isOn ? 1 : 0).ToString(), "UID", Globals.logUser.id.ToString() });
                    string qid = TempDB.single("INSERT INTO questions (question, answers, dlevel, prive, uid) VALUES (@Question, @Answers, @Dlevel, @Prive, @UID); select last_insert_id();");

                    int qAddTag = 0;
                    string[] tags = txtAddTags.Text.TrimEnd(',').Split(',');
                    foreach (string tag in tags)
                    {
                        TempDB.bind(new string[] { "TAG", tag, "QID", qid });
                        qAddTag += TempDB.nQuery("INSERT INTO tags (nametag, qid) VALUES (@TAG, @QID)");
                    }

                    if (string.IsNullOrEmpty(qid) == false && qAddTag > 0)
                    {
                        Utilities.notifyThem(ntfAdd, "Successfull Added Question !", NotificationBox.Type.Success);
                        Functionality.RefreshMyQuestions();
                        Utilities.InvokeMe(txtAddQ, () =>
                         {
                             txtAddQ.Text = "";
                         });
                        Utilities.InvokeMe(dgvAnswerlist, () =>
                        {
                            dgvAnswerlist.Rows.Clear();
                            dgvAnswerlist.Refresh();
                        });
                        Utilities.InvokeMe(txtAddTags, () =>
                        {
                            txtAddTags.Text = "";
                        });
                        Utilities.InvokeMe(difficultyLvl, () =>
                        {
                            difficultyLvl.Value = 1;
                        });
                        Utilities.InvokeMe(switchPrivate, () =>
                        {
                            switchPrivate.isOn = false;
                        });
                        Utilities.InvokeMe(txtAnswer, () =>
                        {
                            txtAnswer.Text = "";
                        });
                        Utilities.InvokeMe(switchCorrectAnswer, () =>
                        {
                            switchCorrectAnswer.isOn = true;
                        });

                        Functionality.LoadTags(autocompleteMenu1);
                    }
                }).Start();

            }
        }

        private void btnAddAnswer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAnswer.Text.Trim()))
            {
                Utilities.notifyThem(ntfAdd, "Write your answer first.", NotificationBox.Type.Warning);
            }
            else
            {
                dgvAnswerlist.Rows.Add(txtAnswer.Text.TrimEnd().TrimStart(), switchCorrectAnswer.isOn);
                txtAnswer.Text = "";
                switchCorrectAnswer.isOn = true;
            }
        }

        private void dgvAnswerlist_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvAnswerlist.CurrentCell != null && dgvAnswerlist.CurrentCell.ColumnIndex == dgvAnswerlist.Columns["answer"].Index)
            {
                Control cntObject = new Control();
                e.Control.TextChanged += new EventHandler((object sse, EventArgs se) => Cell_TextChanged(sse, dgvAnswerlist, cntObject));
                cntObject = e.Control;
                cntObject.TextChanged += (object sse, EventArgs se) => Cell_TextChanged(sse, dgvAnswerlist, cntObject);
            }
        }

        private void Cell_TextChanged(object sender,DataGridView dgv, Control e)
        {
            if (e != null)
            {
                Utilities.txtBoxReplaceNewLine((TextBox)e);
                dgv.CurrentCell.Value = e.Text;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTags.Text))
            {
                Utilities.notifyThem(ntbfindQ, "You must add some tags first.", NotificationBox.Type.Error);
            }
            else
            {
                Utilities.runInThread(() =>
                {
                    DB TempDB = Utilities.AsyncDB(true);
                    string[] tags = txtTags.Text.TrimEnd(',').Split(',');
                    string conQuery = "";
                    for(int i = 0;i< tags.Length;i++)
                    {
                        conQuery += "@" + i + ",";
                    }

                    string fromall = "";
                    if (switchFindAll.isOn)
                    {
                        fromall = "(prive = 0 and uid != " + Globals.logUser.id + ") or ";
                    }

                    DataTable dt = new DataTable();

                    TempDB.qBind(tags);
                    if (!switchAllTags.isOn) {
                        dt = TempDB.query("select * from questions where (" + fromall + "uid = " + Globals.logUser.id + ") and dlevel >= "+ numericMin.Value + " and dlevel <= " + numericMax.Value + " and id in(select distinct qid from tags where nametag in (" + conQuery.TrimEnd(',') + "));");
                    }
                    else
                    {
                        dt = TempDB.query("SELECT * FROM  `questions` WHERE (" + fromall + "uid = " + Globals.logUser.id + ") and dlevel >= " + numericMin.Value + " and dlevel <= " + numericMax.Value + " and id IN (SELECT qid FROM  `tags` WHERE nametag IN (SELECT nametag FROM tags WHERE nametag IN (" + conQuery.TrimEnd(',') + ")) GROUP BY qid HAVING COUNT( qid ) = " + tags.Length +")");
                    }

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dt.Columns[i].ReadOnly = true;
                    }

                    Utilities.InvokeMe(dgvFoundQ, () =>
                    {
                        dgvFoundQ.DataSource = dt;
                        dgvFoundQ.Columns[0].Visible = true;
                        dgvFoundQ.Columns[0].Width = 50;
                        dgvFoundQ.Columns[1].Visible = false;
                        dgvFoundQ.Columns[2].HeaderText = "Questions";
                        dgvFoundQ.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                        dgvFoundQ.Columns[2].Width = 410;
                        dgvFoundQ.Columns[3].Visible = false;
                        dgvFoundQ.Columns[4].HeaderText = "Difficulty";
                        dgvFoundQ.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvFoundQ.Columns[4].Width = 60;
                        dgvFoundQ.Columns[5].Visible = false;
                        dgvFoundQ.Columns[6].Visible = false;
                        for (int i = 0; i < dgvFoundQ.Rows.Count; i++)
                        {
                            dgvFoundQ.Rows[i].Cells[0].Value = "True";
                        }
                    });

                    if (dt.Rows.Count < 1) {
                        DataTable da = new DataTable();
                        Utilities.InvokeMe(dgvFoundQ, () =>
                        {
                            dgvFoundQ.DataSource = da;
                            dgvFoundQ.Columns[0].Visible = false;
                        });
                    }

                    Utilities.notifyThem(ntbfindQ, "Found " + dt.Rows.Count + " Questions.", NotificationBox.Type.Notice);

                }).Start();
            }
        }

        private void btnMakeTest_Click(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            for (int i = (dgvFoundQ.Rows.Count-1); i >= 0 ; i--)
            {
                dgvFoundQ.Rows.RemoveAt(i);
            }

            txtTags.Text = "";
            switchAllTags.isOn = false;
            switchFindAll.isOn = true;
            numericMin.Value = 1;
            numericMax.Value = 5;
            Utilities.notifyThem(ntbfindQ, "Cleared Search result and settings !", NotificationBox.Type.Other);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            for (int i = (dgvAnswerlist.Rows.Count - 1); i >= 0; i--)
            {
                dgvAnswerlist.Rows.RemoveAt(i);
            }

            txtAddQ.Text = "";
            txtAddTags.Text = "";
            txtAnswer.Text = "";
            difficultyLvl.Value = 1;
            switchPrivate.isOn = false;
            switchCorrectAnswer.isOn = true;
            Utilities.notifyThem(ntfAdd, "Cleared Question info !", NotificationBox.Type.Other);
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtopassword.Text) == true || string.IsNullOrEmpty(txtnpassword.Text) == true || string.IsNullOrEmpty(txtrnpassword.Text))
            {
                Utilities.notifyThem(ntfP, "You must fill all fields", NotificationBox.Type.Error);
            }
            else if (!txtnpassword.Text.Equals(txtrnpassword.Text))
            {
                Utilities.notifyThem(ntfP, "Different new Password fields", NotificationBox.Type.Error);
            }
            else
            {
                Utilities.runInThread(() =>
                {
                    string qid = Utilities.AsyncDB().single("UPDATE users SET pass='"+ txtnpassword.Text +"' WHERE id=" + Globals.logUser.id);
                }).Start();
                Utilities.notifyThem(ntfP, "Password Changed", NotificationBox.Type.Success);
            }
        }

        private void btnChangeEmail_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtepassword.Text) == true || string.IsNullOrEmpty(txtnemail.Text) == true)
            {
                Utilities.notifyThem(ntfE, "You must fill all fields", NotificationBox.Type.Error);
            }
            else
            {
                Utilities.runInThread(() =>
                {
                    string qid = Utilities.AsyncDB().single("UPDATE users SET email='" + txtnemail.Text + "' WHERE id=" + Globals.logUser.id);
                }).Start();
                Utilities.notifyThem(ntfE, "Email Changed", NotificationBox.Type.Success);
            }
        }

        private void btnChangeSecurity_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtspassword.Text) == true || string.IsNullOrEmpty(txtncode.Text) == true)
            {
                Utilities.notifyThem(ntfC, "You must fill all fields", NotificationBox.Type.Error);
            }
            else
            {
                Utilities.runInThread(() =>
                {
                    string qid = Utilities.AsyncDB().single("UPDATE users SET securitycode='" + txtncode.Text + "' WHERE id=" + Globals.logUser.id);
                }).Start();
                Utilities.notifyThem(ntfC, "Security Code Changed", NotificationBox.Type.Success);
            }
        }
    }
}
