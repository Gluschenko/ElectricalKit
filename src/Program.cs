// © Alexander Gluschenko, 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Anvil.Net;
using ElectricalKit;

namespace ElectricalKit
{
    class Program
    {
        public static Form MainForm;
        public static string Version = "1.0";
        public static string MainFormTitle = "Расчет цепи";

        //Схема
        public static PictureBox SchemePictureBox;
        public static Bitmap SchemePicture;
        public static Graphics SchemeGraph;

        public static string SchemeMarkup = Utils.ExampleScheme(0);

        public static string LastSchemeMarkup = "";//Для отслеживания изменений
        

        //Точка входа
        [STAThread]
        static void Main() 
        {
            UI.Start();

            ///

            MainForm = UI.CreateForm(new Rect(50, 10, 850, 700), string.Format("{0} (v {1})", MainFormTitle, Version));
            MainForm.MaximumSize = new Size(850, 700);
            CreateUI();
            CreateSchemePucture();

            ///

            Apply(); //Применение, расчет, рисование

            ///

            Application.Run(MainForm); //Запуск формы
        }

        public static void CreateUI() 
        {
            UI.Style = UI.DefaultStyle;

            SchemePictureBox = UI.CreatePictureBox(new Rect(0, 0, MainForm.ClientSize.Width, 350), UI.DefaultAnchor, "SchemePicture");
            MainForm.Controls.Add(SchemePictureBox);

            ///

            UI.Style = CommonUI.CommonStyle;

            ///

            //Кнопки навигации

            UI.Append(UI.CreatePanel(new Rect(0, 350, MainForm.ClientSize.Width, 50), UI.DefaultAnchor, "ButtonBar"), MainForm);

            UI.Append(UI.CreateButton(new Rect(5, 5, 150, 40), UI.DefaultAnchor, "MarkupButton", "Схема", delegate()
            {
                UI.Controls["MarkupPanel"].Visible = true;
                UI.Controls["ConfigPanel"].Visible = false;
                UI.Controls["ChainPanel"].Visible = false;
                UI.Controls["CalculationPanel"].Visible = false;
            }), UI.Controls["ButtonBar"]);

            UI.Append(UI.CreateButton(new Rect(160, 5, 150, 40), UI.DefaultAnchor, "ConfigButton", "Источник", delegate()
            {
                UI.Controls["MarkupPanel"].Visible = false;
                UI.Controls["ConfigPanel"].Visible = true;
                UI.Controls["ChainPanel"].Visible = false;
                UI.Controls["CalculationPanel"].Visible = false;
            }), UI.Controls["ButtonBar"]);

            UI.Append(UI.CreateButton(new Rect(315, 5, 150, 40), UI.DefaultAnchor, "ChainButton", "Расчет", delegate()
            {
                UI.Controls["MarkupPanel"].Visible = false;
                UI.Controls["ConfigPanel"].Visible = false;
                UI.Controls["ChainPanel"].Visible = true;
                UI.Controls["CalculationPanel"].Visible = true;
            }), UI.Controls["ButtonBar"]);

            UI.Append(UI.CreateButton(new Rect(MainForm.ClientSize.Width - 205, 5, 200, 40), UI.DefaultAnchor, "ApplyButton", "Применить", delegate()
            {
                Apply();
            }), UI.Controls["ButtonBar"]);

            ///Панели

            UI.Append(UI.CreatePanel(new Rect(0, 400, MainForm.ClientSize.Width, 260), UI.DefaultAnchor, "MarkupPanel"), MainForm);
            UI.Append(UI.CreatePanel(new Rect(0, 400, MainForm.ClientSize.Width, 260), UI.DefaultAnchor, "ConfigPanel"), MainForm);
            UI.Append(UI.CreatePanel(new Rect(0, 400, MainForm.ClientSize.Width / 2 - 100, 260), UI.DefaultAnchor, "ChainPanel"), MainForm);
            UI.Append(UI.CreatePanel(new Rect(MainForm.ClientSize.Width / 2 - 100, 400, MainForm.ClientSize.Width / 2 + 100, 260), UI.DefaultAnchor, "CalculationPanel"), MainForm);

            //Состояния панелей

            UI.Controls["MarkupPanel"].Visible = true;
            UI.Controls["ConfigPanel"].Visible = false;
            UI.Controls["ChainPanel"].Visible = false;
            UI.Controls["CalculationPanel"].Visible = false;


            //Панель верстки схемы
            for (int i = 0; i < 3; i++ ) //Вывод образцов схем
            {
                int[] XPos = new int[] {
                    5,
                    138,
                    271,
                };

                int SchemeId = i;

                UI.Append(UI.CreateButton(new Rect(XPos[i], 220, 130, 35), UI.DefaultAnchor, "Example" + i, "Шаблон #" + (i + 1), delegate()
                {
                    UI.Controls["MarkupText"].Text = Utils.ExampleScheme(SchemeId);
                    Apply();
                }), UI.Controls["MarkupPanel"]);
            }

            UI.Append(UI.CreateButton(new Rect(404, 220, 120, 35), UI.DefaultAnchor, "RandomExample", "Случайная", delegate()
            {
                UI.Controls["MarkupText"].Text = Utils.RandomScheme();
                Apply();
            }), UI.Controls["MarkupPanel"]);


            UI.Append(UI.CreateButton(new Rect(MainForm.ClientSize.Width - 40, 220, 35, 35), UI.DefaultAnchor, "Info", "i", delegate()
            {
                MessageBox.Show("Целевая среда: .NET 3.5\nЯзык: C#\nВерсия: " + Version + "\nРазработчик: Глущенко Александр", "Информация");
            }), UI.Controls["MarkupPanel"]);
            UI.Controls["Info"].Font = new Font("Times New Roman", 16f);

            UI.Append(UI.CreateTextBox(new Rect(5, 5, MainForm.ClientSize.Width - 10, 210), UI.DefaultAnchor, "MarkupText", SchemeMarkup, 10000, "SegoeUI", 17), UI.Controls["MarkupPanel"]);
            ///

            //Понфигурация цепи
            UI.Append(UI.CreateLabel(new Rect(5, 5, 0, 0), "U_Label", "Входное напряжение (В)"), UI.Controls["ConfigPanel"]);
            UI.Append(UI.CreateTextField(new Rect(10, 40, 250, 30), UI.DefaultAnchor, "U", "220"), UI.Controls["ConfigPanel"]);
            //
            UI.Append(UI.CreateCheckBox(new Rect(10, 80, 200, 30), UI.DefaultAnchor, "DC", "Постоянный ток", true), UI.Controls["ConfigPanel"]);
            //
            UI.Append(UI.CreateLabel(new Rect(5, 110, 0, 0), "F_Label", "Частота (Гц)"), UI.Controls["ConfigPanel"]);
            UI.Append(UI.CreateTextField(new Rect(10, 145, 250, 30), UI.DefaultAnchor, "F", "50"), UI.Controls["ConfigPanel"]);
            
            UI.Controls["F_Label"].Visible = UI.Controls["F"].Visible = false;
            //
            System.Windows.Forms.CheckBox CB = (CheckBox)UI.Controls["DC"];
            CB.CheckedChanged += delegate {
                UI.Controls["F_Label"].Visible = UI.Controls["F"].Visible = !CB.Checked;
            };

            ///
        }

        public static void CreateEditFields() //Поля создаются с именем, соответствующим имени потребителя
        {
            UI.ControlsOf(UI.Controls["ChainPanel"]).Clear();//Работает

            ///

            int InitialParam = 1;//Инкремент для дефолтного заполнения полей

            int HeightOffset = 5;
            for (int i = 0; i < Scheme.Points.Count(); i++ ) //Точки
            {
                UI.Append(UI.CreateLabel(new Rect(5, HeightOffset, 0, 0), "Point_" + i, "Участок цепи #" + (i + 1)), UI.Controls["ChainPanel"]);
                HeightOffset += 30;

                Consumer[][] SchemePoint = Scheme.Points[i];

                for (int line = 0; line < SchemePoint.Length; line++ ) //Подцепи
                {
                    for (int j = 0; j < SchemePoint[line].Length; j++) //Потребители
                    {
                        Consumer Con = SchemePoint[line][j];

                        UI.Append(UI.CreateLabel(new Rect(10, HeightOffset, 50, 30), "Point_" + i + "_" + Con.Name, Con.Name, "SegoeUI", 12), UI.Controls["ChainPanel"]);
                        UI.Append(UI.CreateTextField(new Rect(60, HeightOffset, 150, 30), UI.DefaultAnchor, Con.Name, InitialParam.ToString()), UI.Controls["ChainPanel"]);
                        UI.Append(UI.CreateLabel(new Rect(215, HeightOffset, 50, 30), "Point_" + i + "_" + Con.Name + "_Param", Scheme.UnitByName(Con.Name), "SegoeUI", 12), UI.Controls["ChainPanel"]);

                        HeightOffset += 35;
                        ///
                        InitialParam++;//Инкрементируем
                    }
                }
            }
        }

        public static void CreateOutput() 
        {
            UI.ControlsOf(UI.Controls["CalculationPanel"]).Clear();

            //

            int Height = 5;

            //Вывод данных цепи

            UI.Append(UI.CreateLabel(new Rect(5, Height, 350, 30), "Out_Label", "Результаты"), UI.Controls["CalculationPanel"]);
            Height += 35;

            string[] Output = new string[] {
                "Output_U", "Dir", "Напряжение (U): " + Physics.Round(Physics.U) + " В",
                "Output_F", "Alt", "Частота (F): " + Physics.Round(Physics.F) + " Гц",
                "Output_I", "Dir", "Сила тока (I): " + Physics.Round(Physics.I) + " А",
                "Output_R", "Dir", "Сопротивление (R): " + Physics.Round(Physics.R) + " Ом",
                "Output_XC", "Alt", "Ёмкостное сопротивление (XC): " + Physics.Round(Physics.XC) + " Ом",
                "Output_XL", "Alt", "Индуктивное сопротивление (XL): " + Physics.Round(Physics.XL) + " Ом",
                "Output_Z", "Alt", "Полное сопротивление (Z): " + Physics.Round(Physics.Z) + " Ом",
                "Output_P", "Dir", "Мощность (P): " + Physics.Round(Physics.P) + " Вт",
                "Output_Q", "Alt", "Реактивная мощность (Q): " + Physics.Round(Physics.Q) + " ВАр",
                "Output_S", "Alt", "Полная мощность (S): " + Physics.Round(Physics.S) + " ВА",
                "Output_Fi", "Alt", "λ: " + Physics.Round(Physics.CosFi * 100) + " %",
                "Output_CosFi", "Alt", "Cos(φ): " + Physics.CosFi + " ", //Пробелы обязаны быть
                "Output_SinFi", "Alt", "Sin(φ): " + Physics.SinFi + " ",
            };

            for (int i = 0; i < Output.Length; i += 3)
            {
                bool isDisplay = true;
                if (Physics.isDirect) isDisplay = (Output[i + 1] == "Dir");

                if (isDisplay)
                {
                    UI.Append(UI.CreateLabel(new Rect(10, Height, 350, 30), Output[i], Output[i + 2], "SegoeUI", 12), UI.Controls["CalculationPanel"]);
                    Height += 30;
                }
            }

            //Вывод данных потребителей

            for (int i = 0; i < Scheme.Points.Count(); i++) //Точки
            {
                Consumer[][] SchemePoint = Scheme.Points[i];

                for (int line = 0; line < SchemePoint.Length; line++) //Подцепи
                {
                    for (int j = 0; j < SchemePoint[line].Length; j++) //Потребители
                    {
                        Consumer Con = SchemePoint[line][j];
                        //
                        string[] ConsumerOutput = new string[] {
                            "Output_R_" + Con.Name, "Dir", "Сопротивление (R): " + Physics.Round(Con.R) + " Ом",
                            "Output_C_" + Con.Name, "Dir", "Ёмкость (C): " + Physics.Round(Con.C) + " μФ",
                            "Output_L_" + Con.Name, "Dir", "Индуктивность (L): " + Physics.Round(Con.L) + " Гн",
                            "Output_I_" + Con.Name, "Dir", "Сила тока (I): " + Physics.Round(Con.I) + " А",
                            "Output_U_" + Con.Name, "Dir", "Падение (U): " + Physics.Round(Con.U) + " В",
                            "Output_P_" + Con.Name, "Dir", "Мощность (P): " + Physics.Round(Con.P) + " Вт",
                        };

                        //

                        UI.Append(UI.CreateLabel(new Rect(5, Height, 350, 30), "Output_" + Con.Name, Con.Name), UI.Controls["CalculationPanel"]);
                        Height += 35;

                        for (int с = 0; с < ConsumerOutput.Length; с += 3)
                        {
                            bool isDisplay = true;
                            if (Physics.isDirect) isDisplay = (ConsumerOutput[с + 1] == "Dir");

                            if (isDisplay && !ConsumerOutput[с + 2].Contains(" 0 ") && !ConsumerOutput[с + 2].Contains(" NaN ")) //Вторая часть условия, по сути - костыль. Но мне лень делать это с помощью лишнего элемента в массиве и string.Format()
                            {
                                UI.Append(UI.CreateLabel(new Rect(10, Height, 350, 30), ConsumerOutput[с], ConsumerOutput[с + 2], "SegoeUI", 12), UI.Controls["CalculationPanel"]);
                                Height += 30;
                            }
                        }


                        //
                        Scheme.Points[i][line][j] = Con;
                    }
                }
            }
        }

        ///

        public static void Apply() //Основная точка применения
        {
            try
            {
                SchemeMarkup = UI.Controls["MarkupText"].Text; //Присвоение данных схемы

                ///

                ApplyPhysics();//Применение параметров цепи
                Scheme.Parse(SchemeMarkup, SchemePicture);//Парсинг и рисование схемы на картинке
                if (SchemeMarkup != LastSchemeMarkup) CreateEditFields();//Создание полей параметров для потребителей
                ApplyConsumers();//Применение параметров потребителей
                //
                Physics.CalculateAll();//Физические расчеты
                //
                CreateOutput();//Заполнение панели вывода

                ///

                LastSchemeMarkup = SchemeMarkup; //Для последующего сравнения (отслеживание отличий)
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Ошибка введённых данных");
            }
        }

        ///

        public static void ApplyConsumers() //Перебиваем потребители и раздаем параметры из формы ввода
        {
            for (int i = 0; i < Scheme.Points.Count(); i++) //Точки
            {
                Consumer[][] SchemePoint = Scheme.Points[i];

                for (int line = 0; line < SchemePoint.Length; line++) //Подцепи
                {
                    for (int j = 0; j < SchemePoint[line].Length; j++) //Потребители
                    {
                        Consumer Con = SchemePoint[line][j];

                        string ParamValue = UI.Controls[Con.Name].Text.Replace(".", ",");
                        Scheme.SetConsumer(ref Con, double.Parse(ParamValue)); //Поля с данными названы так же, как и потребители

                        Scheme.Points[i][line][j] = Con; //Странно, по идее должна передаваться ссылка везде, но для применения параметров, их надо вот так закостылить
                    }
                }
            }
        }

        public static void ApplyPhysics() //Парсим и присваеваем входные данные для класса Physics
        {
            Physics.U = double.Parse(UI.Controls["U"].Text);
            Physics.F = double.Parse(UI.Controls["F"].Text);

            System.Windows.Forms.CheckBox DC = (CheckBox)UI.Controls["DC"];
            Physics.isDirect = DC.Checked;
        }

        public static void CreateSchemePucture() 
        {
            SchemePicture = new Bitmap(SchemePictureBox.Width, SchemePictureBox.Height);
            SchemePictureBox.Image = SchemePicture;
            SchemeGraph = Graphics.FromImage(SchemePicture);

            Scheme.Clear(SchemePicture);
        }
    }
}
