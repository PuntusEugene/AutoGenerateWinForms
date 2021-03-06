﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Kapral.WinFormsGenerate.UserControls;

namespace Kapral.WinFormsGenerate
{
    public class WinFormGenerating
    {
        public Form GetForm(object exemplar)
        {
            var typeClass = exemplar.GetType();

            if (!typeClass.IsClass)
                throw new WinFormGeneratedException(Properties.Resource.ExemplarNotClass);

            var form = new Form()
            {
                Size = new Size(400, 160),
                MinimumSize = new Size(400, 160),
                StartPosition = FormStartPosition.CenterParent,
            };

            var panel = new Panel()
            {
                Size = new Size(360, 58),
                Location = new Point(12, 12),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            form.Controls.Add(panel);

            var button = new Button()
            {
                Text = "OK",
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                DialogResult = DialogResult.OK,
                Size = new Size(100, 25)
            };
            button.Location = new Point(form.Size.Width - button.Size.Width - 28, form.Size.Height - button.Size.Height - 51);

            form.Controls.Add(button);

            var properties = typeClass.GetProperties(BindingFlags.GetProperty |
                                    BindingFlags.SetProperty |
                                    BindingFlags.Instance |
                                    BindingFlags.Public |
                                    BindingFlags.DeclaredOnly);

            var locationY = 12;
            foreach (var propertyInfo in properties)
            {
                var formControlAttribute = propertyInfo.GetCustomAttributes(typeof(FormControlAttribute), true).FirstOrDefault() as FormControlAttribute;

                if (formControlAttribute == null)
                    continue;

                var label = new Label()
                {
                    Text = formControlAttribute.LabelText,
                    Location = new Point(12, locationY),
                    Size = new Size(100, 30),
                    AutoSize = false
                };

                var sizeElement = panel.Size.Width - 20;
                var paddingLeft = 6;

                if (formControlAttribute.IsLabel)
                {
                    panel.Controls.Add(label);
                    sizeElement -= label.Size.Width;
                    paddingLeft += label.Location.X + label.Size.Width;
                }

                switch (formControlAttribute.Type)
                {
                    case FormControlType.TextBox:
                        var textBox = new TextBox()
                        {
                            Size = new Size(sizeElement, 30),
                            Location = new Point(paddingLeft, locationY),
                            Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left
                        };
                        textBox.DataBindings.Add("Text", exemplar, propertyInfo.Name, false, DataSourceUpdateMode.OnPropertyChanged);
                        panel.Controls.Add(textBox);
                        break;

                    case FormControlType.ComboBox:
                        var comboBox = new ComboBox()
                        {
                            Size = new Size(sizeElement, 30),
                            Location = new Point(paddingLeft, locationY),
                            DataSource = formControlAttribute.DataSourse,
                            Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left
                        };
                        comboBox.DataBindings.Add("SelectedItem", exemplar, propertyInfo.Name, false, DataSourceUpdateMode.OnPropertyChanged);
                        panel.Controls.Add(comboBox);
                        break;

                    case FormControlType.ConnectionString:
                        var dataBaseConfig = new DataBaseConfigureControl()
                        {
                            Size = new Size(sizeElement, 170),
                            Location = new Point(paddingLeft, locationY),
                            Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left
                        };
                        dataBaseConfig.DataBindings.Add("ConnectionString", exemplar, propertyInfo.Name, false, DataSourceUpdateMode.OnPropertyChanged);
                        panel.Controls.Add(dataBaseConfig);
                        locationY += 140;
                        break;
                }

                locationY += 31;
            }

            var maxHeight = form.Size.Height + locationY + 10 - panel.Size.Height;
            maxHeight = maxHeight < 800 ? maxHeight : 800;
            form.Size = new Size(form.Size.Width, maxHeight);

            return form;
        }
    }
}
