﻿namespace DSUScheduleBuilder.Main_Menu
{
    partial class UserSettings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.acctOptionsLabel = new System.Windows.Forms.Label();
            this.currPasswordLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.ConfirmLabel = new System.Windows.Forms.Label();
            this.CurrPasswordTxt = new System.Windows.Forms.TextBox();
            this.PasswordTxt = new System.Windows.Forms.TextBox();
            this.ConfirmPasswordTxt = new System.Windows.Forms.TextBox();
            this.UpdatePasswordBtn = new System.Windows.Forms.Button();
            this.DeleteAccountBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // acctOptionsLabel
            // 
            this.acctOptionsLabel.AutoSize = true;
            this.acctOptionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.acctOptionsLabel.Location = new System.Drawing.Point(23, 24);
            this.acctOptionsLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.acctOptionsLabel.Name = "acctOptionsLabel";
            this.acctOptionsLabel.Size = new System.Drawing.Size(167, 24);
            this.acctOptionsLabel.TabIndex = 0;
            this.acctOptionsLabel.Text = "Account Settings";
            // 
            // currPasswordLabel
            // 
            this.currPasswordLabel.AutoSize = true;
            this.currPasswordLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currPasswordLabel.Location = new System.Drawing.Point(24, 63);
            this.currPasswordLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.currPasswordLabel.Name = "currPasswordLabel";
            this.currPasswordLabel.Size = new System.Drawing.Size(132, 18);
            this.currPasswordLabel.TabIndex = 1;
            this.currPasswordLabel.Text = "Current Password:";
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passwordLabel.Location = new System.Drawing.Point(77, 94);
            this.passwordLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(79, 18);
            this.passwordLabel.TabIndex = 2;
            this.passwordLabel.Text = "Password:";
            // 
            // ConfirmLabel
            // 
            this.ConfirmLabel.AutoSize = true;
            this.ConfirmLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConfirmLabel.Location = new System.Drawing.Point(24, 131);
            this.ConfirmLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ConfirmLabel.Name = "ConfirmLabel";
            this.ConfirmLabel.Size = new System.Drawing.Size(136, 18);
            this.ConfirmLabel.TabIndex = 3;
            this.ConfirmLabel.Text = "Confirm Password:";
            // 
            // CurrPasswordTxt
            // 
            this.CurrPasswordTxt.BackColor = System.Drawing.Color.Gray;
            this.CurrPasswordTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CurrPasswordTxt.ForeColor = System.Drawing.Color.White;
            this.CurrPasswordTxt.Location = new System.Drawing.Point(161, 61);
            this.CurrPasswordTxt.Margin = new System.Windows.Forms.Padding(2);
            this.CurrPasswordTxt.Name = "CurrPasswordTxt";
            this.CurrPasswordTxt.ShortcutsEnabled = false;
            this.CurrPasswordTxt.Size = new System.Drawing.Size(150, 20);
            this.CurrPasswordTxt.TabIndex = 10;
            // 
            // PasswordTxt
            // 
            this.PasswordTxt.BackColor = System.Drawing.Color.Gray;
            this.PasswordTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PasswordTxt.ForeColor = System.Drawing.Color.White;
            this.PasswordTxt.Location = new System.Drawing.Point(161, 95);
            this.PasswordTxt.Margin = new System.Windows.Forms.Padding(2);
            this.PasswordTxt.Name = "PasswordTxt";
            this.PasswordTxt.Size = new System.Drawing.Size(150, 20);
            this.PasswordTxt.TabIndex = 11;
            // 
            // ConfirmPasswordTxt
            // 
            this.ConfirmPasswordTxt.BackColor = System.Drawing.Color.Gray;
            this.ConfirmPasswordTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ConfirmPasswordTxt.ForeColor = System.Drawing.Color.White;
            this.ConfirmPasswordTxt.Location = new System.Drawing.Point(161, 132);
            this.ConfirmPasswordTxt.Margin = new System.Windows.Forms.Padding(2);
            this.ConfirmPasswordTxt.Name = "ConfirmPasswordTxt";
            this.ConfirmPasswordTxt.Size = new System.Drawing.Size(150, 20);
            this.ConfirmPasswordTxt.TabIndex = 12;
            // 
            // UpdatePasswordBtn
            // 
            this.UpdatePasswordBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UpdatePasswordBtn.Location = new System.Drawing.Point(66, 171);
            this.UpdatePasswordBtn.Name = "UpdatePasswordBtn";
            this.UpdatePasswordBtn.Size = new System.Drawing.Size(216, 35);
            this.UpdatePasswordBtn.TabIndex = 13;
            this.UpdatePasswordBtn.Text = "Update Password";
            this.UpdatePasswordBtn.UseVisualStyleBackColor = true;
            this.UpdatePasswordBtn.Click += new System.EventHandler(this.UpdatePasswordBtn_Click);
            // 
            // DeleteAccountBtn
            // 
            this.DeleteAccountBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteAccountBtn.Location = new System.Drawing.Point(66, 519);
            this.DeleteAccountBtn.Name = "DeleteAccountBtn";
            this.DeleteAccountBtn.Size = new System.Drawing.Size(216, 35);
            this.DeleteAccountBtn.TabIndex = 14;
            this.DeleteAccountBtn.Text = "Delete Account";
            this.DeleteAccountBtn.UseVisualStyleBackColor = true;
            this.DeleteAccountBtn.Click += new System.EventHandler(this.DeleteAccountBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(27, 473);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 24);
            this.label1.TabIndex = 20;
            this.label1.Text = "Danger Zone!";
            // 
            // UserSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DeleteAccountBtn);
            this.Controls.Add(this.UpdatePasswordBtn);
            this.Controls.Add(this.ConfirmPasswordTxt);
            this.Controls.Add(this.PasswordTxt);
            this.Controls.Add(this.CurrPasswordTxt);
            this.Controls.Add(this.ConfirmLabel);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.currPasswordLabel);
            this.Controls.Add(this.acctOptionsLabel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "UserSettings";
            this.Size = new System.Drawing.Size(872, 606);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label acctOptionsLabel;
        private System.Windows.Forms.Label currPasswordLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.Label ConfirmLabel;
        private System.Windows.Forms.TextBox CurrPasswordTxt;
        private System.Windows.Forms.TextBox PasswordTxt;
        private System.Windows.Forms.TextBox ConfirmPasswordTxt;
        private System.Windows.Forms.Button UpdatePasswordBtn;
        private System.Windows.Forms.Button DeleteAccountBtn;
        private System.Windows.Forms.Label label1;
    }
}
