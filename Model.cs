using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace TubeSheetRobot
{
    public class Model : INotifyPropertyChanged
    {
        #region Fields
        private string _StartingNode;
        private short _StartingAngle;
        private string _EndNode;
        private string _Path;
        private ObservableCollection<short> _StartingAngleCollection = new ObservableCollection<short>();
        private TubeM StNode;
        private TubeM EnNode;
        private TubeM MainPincer1;
        private TubeM MainPincer2;
        private TubeM SidePincer1;
        private TubeM SidePincer2;
        short TranslationMainUp;
        short TranslationMainRight;
        short RotationMain;
        short RotationSide;
        int RotationMax = 0;
        List<TubeM> pathList = new List<TubeM>();
        #endregion
        #region Properties
        private ObservableCollection<TubeM> Tubes { get; set; }
        public ObservableCollection<short> StartingAngleCollection
        {
            get
            {
                return _StartingAngleCollection;
            }
            set
            {
                _StartingAngleCollection = value;
                OnPropertyChanged("StartingAngleCollection");
            }
        }
        public short StartingAngle
        {
            get
            {
                return _StartingAngle;
            }
            set
            {
                _StartingAngle = value;
                RotationMain = _StartingAngle;
                ResetCoordinates();
                OnPropertyChanged("StartingAngle");
            }
        }
        public string StartingNode
        {
            get
            {
                return _StartingNode;
            }
            set
            {
                _StartingNode = value;
                OnPropertyChanged("StartingNode");
            }
        }
        public string EndNode
        {
            get
            {
                return _EndNode;
            }
            set
            {
                _EndNode = value;
                OnPropertyChanged("EndNode");
            }
        }

        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                OnPropertyChanged("Path");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region Constructor
        public Model()
        {
            StartingNode = "None";
            EndNode = "None";
            StNode = null;
            EnNode = null;
            StartingAngleCollection.Add(45);
            StartingAngleCollection.Add(135);
            StartingAngleCollection.Add(225);
            StartingAngleCollection.Add(315);
        }
        #endregion
        #region Methods
        public void SetCoordinates(TubeM node)
        {
            if (!(StNode != node && node.ColorProp != "Black"))
            {
                return;
            }
            if (StartingNode == "None")
            {
                StNode = node;
                StartingNode = node.ToolTip;
                SetStartPincers();
            }
            else
            {
                EnNode = node;
                EndNode = node.ToolTip;
                this.FindPath();
            }
        }

        private void SetStartPincers()
        {
            var mainPincer1row = -1 * Math.Round(Math.Cos(Math.PI * StartingAngle / 180)) + StNode.Row;
            var mainPincer1col = -1 * Math.Round(Math.Sin(Math.PI * StartingAngle / 180)) + StNode.Column;
            var mainPincer2row = -15 * Math.Round(Math.Cos(Math.PI * StartingAngle / 180)) + StNode.Row;
            var mainPincer2col = -15 * Math.Round(Math.Sin(Math.PI * StartingAngle / 180)) + StNode.Column;
            MainPincer1 = Tubes.Where(x => x.Row == mainPincer1row).Where(x => x.Column == mainPincer1col).Where(x=>x.ColorProp!="Black").FirstOrDefault();
            MainPincer2 = Tubes.Where(x => x.Row == mainPincer2row).Where(x => x.Column == mainPincer2col).Where(x => x.ColorProp != "Black").FirstOrDefault();
            if (MainPincer1 == null || MainPincer2 == null)
            {
                ResetCoordinates();
                return;
            }
            RotationMain = StartingAngle;
            RotationSide = (short)(RotationMain + 45);
            var centerRow = (mainPincer1row + mainPincer2row)/2;
            var centerColumn = (mainPincer1col + mainPincer2col)/2;
            var secondPincer1col = centerColumn + 12 * Math.Round(Math.Sin(RotationSide * Math.PI/180));
            var secondPincer2col = centerColumn - 12 * Math.Round(Math.Sin(RotationSide * Math.PI / 180));
            var secondPincer1row = centerRow + 12 * Math.Round(Math.Cos(RotationSide * Math.PI / 180));
            var secondPincer2row = centerRow - 12 * Math.Round(Math.Cos(RotationSide * Math.PI / 180));
            SidePincer1 = Tubes.Where(x => x.Row == secondPincer1row).Where(x => x.Column == secondPincer1col).Where(x => x.ColorProp != "Black").FirstOrDefault();
            SidePincer2 = Tubes.Where(x => x.Row == secondPincer2row).Where(x => x.Column == secondPincer2col).Where(x => x.ColorProp != "Black").FirstOrDefault();            
            if (SidePincer1 == null || SidePincer2 == null)
            {
                secondPincer1col = centerColumn + 12 * Math.Round(Math.Cos(RotationSide * Math.PI / 180));
                secondPincer2col = centerColumn - 12 * Math.Round(Math.Cos(RotationSide * Math.PI / 180));
                secondPincer1row = centerRow + 12 * Math.Round(Math.Sin(RotationSide * Math.PI / 180));
                secondPincer2row = centerRow - 12 * Math.Round(Math.Sin(RotationSide * Math.PI / 180));
                SidePincer1 = Tubes.Where(x => x.Row == secondPincer1row).Where(x => x.Column == secondPincer1col).Where(x => x.ColorProp != "Black").FirstOrDefault();
                SidePincer2 = Tubes.Where(x => x.Row == secondPincer2row).Where(x => x.Column == secondPincer2col).Where(x => x.ColorProp != "Black").FirstOrDefault();
                if (SidePincer1 == null || SidePincer2 == null)
                {
                    ResetCoordinates();
                    return;
                }
                RotationSide = (short)(RotationMain + 135);
            }
        }

        private void FindPath()
        {
            //this.LockedMainPincers = true;
            //this.LockedSidePincers = true;
            TubeM currentPos = StNode;
            int row = currentPos.Row;
            int column = currentPos.Column;            
            ResetColor();
            try
            {
                Pathfinding(currentPos);
            }
            catch
            {
                Path = "Didn't manage to get to the point";
                RotationMain = StartingAngle;
                SetStartPincers();
                TranslationMainUp = 0;
                TranslationMainRight = 0;
                pathList = new List<TubeM>();
            }
            string pathlist = "";
            foreach (var item in pathList)
            {
                if (item != null)
                {
                    pathlist += item.ToolTip + " ";
                }
                else
                {
                    pathlist += "OUT" + " ";
                }
            }
            if (pathlist.Length > 0) { pathlist.Trim(); Path = pathlist; }     
        }

        private void Pathfinding(TubeM currentPos)
        {
            var coordinates = MinusCoordinates(currentPos, EnNode);
            var rows = Math.Abs(coordinates.First());
            var columns = Math.Abs(coordinates.Last());
            if ((rows*rows + columns*columns) < 50) { return; }
            currentPos = CheckMainRotation(currentPos, coordinates);
            coordinates = MinusCoordinates(currentPos, EnNode);
            if ((coordinates.First() * coordinates.First() + coordinates.Last() * coordinates.Last()) < 98) { return; }
            currentPos = ShiftMain(currentPos, coordinates);
            coordinates = MinusCoordinates(currentPos, EnNode);
            if ((coordinates.First() * coordinates.First() + coordinates.Last() * coordinates.Last()) < 98) { return; }
            ShiftSide(currentPos, coordinates);
            RotationMax = 0;
            Pathfinding(currentPos);
        }

        private void ShiftSide(TubeM currentPos, List<int> coordinates)
        {
            //revert translation to zero
            var row1 = SidePincer1.Row;
            var col1 = SidePincer1.Column;
            var row2 = SidePincer2.Row;
            var col2 = SidePincer2.Column;
            int i = TranslationMainUp;
            while (i > 0)
            {
                var rowSide1 = SidePincer1.Row + Math.Round(Math.Cos(Math.PI * RotationMain / 180)) * i;
                var rowSide2 = SidePincer2.Row + Math.Round(Math.Cos(Math.PI * RotationMain / 180)) * i;
                if (Tubes.Where(x => x.Row == rowSide1).Where(y => y.Column == SidePincer1.Column).Where(x => x.ColorProp != "Black").FirstOrDefault() != null &&
                    Tubes.Where(x => x.Row == rowSide2).Where(y => y.Column == SidePincer2.Column).Where(x => x.ColorProp != "Black").FirstOrDefault() != null)
                {
                    SidePincer1 = Tubes.Where(x => x.Row == rowSide1).Where(y => y.Column == SidePincer1.Column).FirstOrDefault();
                    SidePincer2 = Tubes.Where(x => x.Row == rowSide2).Where(y => y.Column == SidePincer2.Column).FirstOrDefault();
                    break;
                }
                i = i - 1;
            }
            TranslationMainUp = (short)(TranslationMainUp - i);
            int j = TranslationMainRight;
            while (j> 0)
            {
                var colSide1 = SidePincer1.Column + Math.Round(Math.Sin(Math.PI * RotationMain / 180)) * i;
                var colSide2 = SidePincer2.Column + Math.Round(Math.Sin(Math.PI * RotationMain / 180)) * i;
                if (Tubes.Where(x => x.Row == SidePincer1.Row).Where(y => y.Column == colSide1).Where(x => x.ColorProp != "Black").FirstOrDefault() != null &&
                    Tubes.Where(x => x.Row == SidePincer2.Row).Where(y => y.Column == colSide2).Where(x => x.ColorProp != "Black").FirstOrDefault() != null)
                {
                    SidePincer1 = Tubes.Where(x => x.Row == SidePincer1.Row).Where(y => y.Column == colSide1).FirstOrDefault();
                    SidePincer2 = Tubes.Where(x => x.Row == SidePincer2.Row).Where(y => y.Column == colSide2).FirstOrDefault();
                    break;
                }
                j = j - 1;
            }
            TranslationMainRight = (short)(TranslationMainRight - j);
        }

        private TubeM ShiftMain(TubeM currentPos, List<int> coordinates)
        {
            var rows = coordinates.First();
            var columns = coordinates.Last();
            if ((rows * rows + columns*columns) > 98)
            {//needs improvement
                int i = 6 - TranslationMainUp;
                while (i > 0)
                {
                    var rowMain1 = MainPincer1.Row + Math.Round(Math.Cos(Math.PI * RotationMain / 180)) * i;
                    var columnMain1 = MainPincer1.Column + Math.Round(Math.Sin(Math.PI * RotationMain / 180)) * i;
                    var rowMain2 = MainPincer2.Row + Math.Round(Math.Cos(Math.PI * RotationMain / 180)) * i;
                    var columnMain2 = MainPincer2.Column + Math.Round(Math.Sin(Math.PI * RotationMain / 180)) * i;
                    if (Tubes.Where(x => x.Row == rowMain1).Where(y => y.Column == columnMain1).Where(x => x.ColorProp != "Black").FirstOrDefault() != null &&
                        Tubes.Where(x => x.Row == rowMain2).Where(y => y.Column == columnMain2).Where(x => x.ColorProp != "Black").FirstOrDefault() != null)
                    {
                        MainPincer1 = Tubes.Where(x => x.Row == rowMain1).Where(y => y.Column == columnMain1).FirstOrDefault();
                        MainPincer2 = Tubes.Where(x => x.Row == rowMain2).Where(y => y.Column == columnMain2).FirstOrDefault();
                        var currentPosrow = currentPos.Row + Math.Round(Math.Cos(Math.PI * RotationMain / 180)) * i;
                        var currentPoscol = currentPos.Column + Math.Round(Math.Sin(Math.PI * RotationMain / 180)) * i;
                        currentPos = Tubes.Where(x => x.Row == currentPosrow).Where(y => y.Column == currentPoscol).FirstOrDefault();
                        pathList.Add(currentPos);
                        break;
                    }
                    i = i - 1;
                }

                if (i == 0)
                {
                    if (TranslationMainRight < 12)
                    {
                        TranslationMainRight += 1;
                        ShiftMain(currentPos, coordinates);
                    }
                    else
                    {
                        currentPos = TryToRotate(currentPos, 90);
                        ShiftMain(currentPos, coordinates);
                    }
                }
                TranslationMainUp= (short)(TranslationMainUp + i);
            }
            else
            {
                currentPos = EnNode;
            }
            return currentPos;
        }

        private TubeM CheckMainRotation(TubeM currentPos, List<int> coordinates)
        {
            var rows = coordinates.First();
            var columns = coordinates.Last();
            if (RotationMain > 360) { RotationMain = (short)(RotationMain - 360); }
            if (RotationSide > 360) { RotationSide = (short)(RotationSide - 360); }
            if (RotationMain < 0) { RotationMain = (short)(RotationMain + 360); }
            if (RotationSide < 0) { RotationSide = (short)(RotationSide + 360); }
            //if (Math.Abs(rows) <= 5 && Math.Abs(columns) <= 5)
            //{// head can be extended this much if main translation is 0
            //    return currentPos;
            //}
            if (rows >= 0 && columns >= 0 && RotationMain != 45)
            {
                currentPos = TryToRotate(currentPos, -RotationMain + 45);
            }
            else if (rows >= 0 && columns < 0 && RotationMain != 315)
            {
                currentPos = TryToRotate(currentPos, -RotationMain + 315);
            }
            else if (rows < 0 && columns < 0 && RotationMain != 225)
            {
                currentPos = TryToRotate(currentPos, -RotationMain + 225);
            }
            else if (rows < 0 && columns > 0 && RotationMain != 135)
            {
                currentPos = TryToRotate(currentPos, -RotationMain + 135);
            }
            return currentPos;
        }

        private TubeM TryToRotate(TubeM currentPos, int Angle)
        {
            if (RotationMain > 360) { RotationMain = (short)(RotationMain - 360); }
            if (RotationSide > 360) { RotationSide = (short)(RotationSide - 360); }
            if (RotationMain < 0) { RotationMain = (short)(RotationMain + 360); }
            if (RotationSide < 0) { RotationSide = (short)(RotationSide + 360); }
            if (RotationMax == 30) 
            {
                Debug.WriteLine("You are stuck");
                throw new Exception();
            }
            if (Angle < 0) { Angle = 360 + Angle; }
            switch (Angle)
            {
                case 90:
                    var a = RotationSide; 
                    if (RotationMain > RotationSide) { a = (short)(360 + RotationSide); }
                    if (a - RotationMain == 135)
                    {
                        var rowMain1 = MainPincer1.Row + 14* Math.Round(Math.Cos(Math.PI * RotationSide / 180));
                        var columnMain1 = MainPincer1.Column + 14* Math.Round(Math.Sin(Math.PI * RotationSide / 180));
                        var rowMain2 = MainPincer2.Row - 14* Math.Round(Math.Cos(Math.PI * RotationSide / 180));
                        var columnMain2 = MainPincer2.Column - 14* Math.Round(Math.Sin(Math.PI * RotationSide / 180));
                        if (Tubes.Where(x => x.Row == rowMain1).Where(y => y.Column == columnMain1).Where(x => x.ColorProp != "Black").FirstOrDefault() != null &&
                        Tubes.Where(x => x.Row == rowMain2).Where(y => y.Column == columnMain2).Where(x => x.ColorProp != "Black").FirstOrDefault() != null)
                        {
                            RotationMain += 90;
                            MainPincer1 = Tubes.Where(x => x.Row == rowMain1).Where(y => y.Column == columnMain1).FirstOrDefault();
                            MainPincer2 = Tubes.Where(x => x.Row == rowMain2).Where(y => y.Column == columnMain2).FirstOrDefault();
                            var currentPosrow = currentPos.Row + 16*Math.Cos(Math.PI * RotationSide / 180);
                            var currentPoscol = currentPos.Column + 16*Math.Sin(Math.PI * RotationSide / 180);
                            currentPos = Tubes.Where(x => x.Row == currentPosrow).Where(y => y.Column == currentPoscol).FirstOrDefault();
                            pathList.Add(currentPos);
                        }
                        else
                        {
                            currentPos = TryToRotate(currentPos, 270);
                            RotationMax += 1;
                        }
                    }                   
                    else
                    {
                        RotationMax += 1;
                        RotateSide(currentPos, 90);
                        currentPos = TryToRotate(currentPos, 90);
                    }
                    return currentPos;
                case 180:
                    currentPos = TryToRotate(currentPos, 90);
                    currentPos = TryToRotate(currentPos, 90);
                    return currentPos;
                case 270:
                    var ab = RotationSide;
                    if (RotationMain > RotationSide) { ab = (short)(360 + RotationSide); }
                    if ((ab - RotationMain) == 45)
                    {
                        var rowMain1 = MainPincer1.Row - 14*Math.Round(Math.Cos(Math.PI * RotationSide / 180));
                        var columnMain1 = MainPincer1.Column - 14* Math.Round(Math.Sin(Math.PI * RotationSide / 180));
                        var rowMain2 = MainPincer2.Row + 14* Math.Round(Math.Cos(Math.PI * RotationSide / 180));
                        var columnMain2 = MainPincer2.Column + 14* Math.Round(Math.Sin(Math.PI * RotationSide / 180));
                        if (Tubes.Where(x => x.Row == rowMain1).Where(y => y.Column == columnMain1).Where(x => x.ColorProp != "Black").FirstOrDefault() != null &&
                        Tubes.Where(x => x.Row == rowMain2).Where(y => y.Column == columnMain2).Where(x => x.ColorProp != "Black").FirstOrDefault() != null)
                        {
                            RotationMain -= 90;
                            MainPincer1 = Tubes.Where(x => x.Row == rowMain1).Where(y => y.Column == columnMain1).FirstOrDefault();
                            MainPincer2 = Tubes.Where(x => x.Row == rowMain2).Where(y => y.Column == columnMain2).FirstOrDefault();
                            var currentPosrow = currentPos.Row - 16* Math.Round(Math.Cos(Math.PI * RotationSide / 180));
                            var currentPoscol = currentPos.Column - 16* Math.Round(Math.Sin(Math.PI * RotationSide / 180));
                            currentPos = Tubes.Where(x => x.Row == currentPosrow).Where(y => y.Column == currentPoscol).FirstOrDefault();
                            pathList.Add(currentPos);
                        }
                        else
                        {
                            currentPos = TryToRotate(currentPos, 90);
                            RotationMax += 1;
                        }
                    }
                    else
                    {
                        RotateSide(currentPos, 270);
                    }
                    return currentPos;
                default:
                    return currentPos;

            }

        }

        private void RotateSide(TubeM currentPos, int Angle)
        {
            var kondi = 1;
            switch (Angle)
            {
                case 270:
                    kondi = -1;
                    break;
                default:
                    kondi = 1;
                    break;
            }
            var centerRow = (SidePincer1.Row + SidePincer2.Row) / 2;
            var centerColumn = (SidePincer1.Column + SidePincer2.Column) / 2;
            var secondPincer1col = centerColumn + (12+TranslationMainUp) *kondi* Math.Round(Math.Sin((RotationSide+90) * Math.PI / 180));
            var secondPincer2col = centerColumn - (12 + TranslationMainUp) * kondi* Math.Round(Math.Sin((RotationSide+90) * Math.PI / 180));
            var secondPincer1row = centerRow + (12 + TranslationMainUp) * kondi* Math.Round(Math.Cos((RotationSide+90) * Math.PI / 180));
            var secondPincer2row = centerRow - (12 + TranslationMainUp) * kondi* Math.Round(Math.Cos((RotationSide+90) * Math.PI / 180));

            if (Tubes.Where(x => x.Row == secondPincer1row).Where(y => y.Column == secondPincer1col).Where(x => x.ColorProp != "Black").FirstOrDefault() != null &&
                    Tubes.Where(x => x.Row == secondPincer2row).Where(y => y.Column == secondPincer2col).Where(x => x.ColorProp != "Black").FirstOrDefault() != null)
            {
                RotationSide = (short)(RotationSide + kondi * 90);
                SidePincer1 = Tubes.Where(x => x.Row == secondPincer1row).Where(y => y.Column == secondPincer1col).FirstOrDefault();
                SidePincer2 = Tubes.Where(x => x.Row == secondPincer2row).Where(y => y.Column == secondPincer2col).FirstOrDefault();
            }
        }

        private List<int> MinusCoordinates(TubeM node1, TubeM node2)
        {
            var newList = new List<int>();
            newList.Add(node2.Row - node1.Row);
            newList.Add(node2.Column - node1.Column);
            return newList;
        }

        private void ResetColor()
        {
            var blueTubes = Tubes.Where(x => x.ColorProp == "Blue");
            foreach (var item in blueTubes)
            {
                item.ColorProp = "Gray";
            }
        }

        private void Pathfinder(TubeM currentPos, int row, int column, List<TubeM> pathList)
        {
            bool rS = false;
            bool cS = false;
            if ((EnNode.Row - currentPos.Row) > 0)
            {
                row += 1;
                rS = true;
            }
            else if ((EnNode.Row - currentPos.Row) < 0)
            {
                row -= 1;
                rS = true;
            }
            if ((EnNode.Column - currentPos.Column) > 0)
            {
                column += 1;
                cS = true;
            }
            else if ((EnNode.Column - currentPos.Column) < 0)
            {
                column -= 1;
                cS = true;
            }
            var currentPos1 = Tubes.Where(x => x.Row == row).Where(y => y.Column == column).Where(x => x.ColorProp != "Black").FirstOrDefault();
            if (currentPos1 == null)
            {
                if (rS && cS)
                {
                    currentPos1 = Tubes.Where(x => x.Row == currentPos.Row).Where(y => y.Column == column).Where(x => x.ColorProp != "Black").FirstOrDefault();
                    if (currentPos1 != null)
                    {
                        currentPos = currentPos1;
                        row = currentPos.Row;
                    }
                    else
                    {
                        currentPos = Tubes.Where(x => x.Row == row).Where(y => y.Column == currentPos.Column).Where(x => x.ColorProp != "Black").FirstOrDefault();
                        column = currentPos.Column;
                    }
                }
                //additional elses must be added
            }
            else { currentPos = currentPos1; }
            pathList.Add(currentPos);
            if (currentPos != EnNode)
            {
                if (currentPos.ColorProp != "Red")
                {
                    currentPos.ColorProp = "Blue";
                }
                Pathfinder(currentPos, row, column, pathList);
            }
        }

        public void ResetCoordinates()
        {
            StartingNode = "None";
            EndNode = "None";
            StNode = null;
            EnNode = null;
            Path = "";
            ResetColor();
            TranslationMainUp = 0;
            TranslationMainRight = 0;
            RotationMax = 0;
            pathList = new List<TubeM>();
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal void CollectAllTubes(ObservableCollection<TubeM> tubes)
        {
            this.Tubes = tubes;
        }

        internal void ConnectTubesheet(TubesheetModel tubesheetModel)
        {
            CollectAllTubes(tubesheetModel.Tubes);
        }
        #endregion
    }
}
