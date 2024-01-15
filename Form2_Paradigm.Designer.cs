namespace Pseudo_RealTime_ALS_BCI
{
    partial class Form2_Paradigm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label_value1 = new System.Windows.Forms.Label();
            this.label_symbol1 = new System.Windows.Forms.Label();
            this.label_value2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.27273F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.72727F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 536F));
            this.tableLayoutPanel2.Controls.Add(this.label_value1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label_symbol1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label_value2, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(109, 229);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1350, 305);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // label_value1
            // 
            this.label_value1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_value1.AutoSize = true;
            this.label_value1.Font = new System.Drawing.Font("微軟正黑體", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_value1.Location = new System.Drawing.Point(3, 0);
            this.label_value1.Name = "label_value1";
            this.label_value1.Size = new System.Drawing.Size(541, 305);
            this.label_value1.TabIndex = 0;
            this.label_value1.Text = "-0000";
            this.label_value1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_value1.Visible = false;
            // 
            // label_symbol1
            // 
            this.label_symbol1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_symbol1.AutoSize = true;
            this.label_symbol1.Font = new System.Drawing.Font("微軟正黑體", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_symbol1.Location = new System.Drawing.Point(550, 0);
            this.label_symbol1.Name = "label_symbol1";
            this.label_symbol1.Size = new System.Drawing.Size(260, 305);
            this.label_symbol1.TabIndex = 2;
            this.label_symbol1.Text = "+";
            this.label_symbol1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_value2
            // 
            this.label_value2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_value2.AutoSize = true;
            this.label_value2.Font = new System.Drawing.Font("微軟正黑體", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_value2.Location = new System.Drawing.Point(816, 0);
            this.label_value2.Name = "label_value2";
            this.label_value2.Size = new System.Drawing.Size(531, 305);
            this.label_value2.TabIndex = 1;
            this.label_value2.Text = "-0000";
            this.label_value2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_value2.Visible = false;
            // 
            // Form2_Paradigm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1568, 762);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "Form2_Paradigm";
            this.Text = "Form_Paradigm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label_value1;
        private System.Windows.Forms.Label label_symbol1;
        private System.Windows.Forms.Label label_value2;
    }
}