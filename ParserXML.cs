using System;
using System.Collections.Generic;
using System.Xml;

namespace TubeSheetRobot
{
    public class ParserXML
    {
        public List<TubeM> listOfElements
        {
            get { return _listOfElements; }
        }
        private List<TubeM> _listOfElements;
        public double diameter{get;set;}
        public double pitch {get;set;}
        public ParserXML()
        {
            _listOfElements = new List<TubeM>();
            diameter = 0;
            pitch = 0;
        }
        public void ParseFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"..\..\Resources\Tubesheet.txt");
            diameter = Convert.ToDouble(doc.SelectSingleNode("TubesheetModel/TubesheetDiameter").InnerText);
            pitch = Convert.ToDouble(doc.SelectSingleNode("TubesheetModel/TubesheetPitch").InnerText);
            var model = new Model();
            var tubes = doc.SelectNodes("TubesheetModel/Tubes/Tube");          
            foreach (XmlElement tube in tubes)
            {
                var TubeM = new TubeM(diameter, pitch, model)
                {
                    Row = Convert.ToInt32(tube.SelectSingleNode("Row").InnerText),
                    Column = Convert.ToInt32(tube.SelectSingleNode("Column").InnerText),
                    ColorProp = tube.SelectSingleNode("Status").InnerText
                };
                _listOfElements.Add(TubeM);
            }
        }
    }
}
