﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace HOK.RoomsToMass.ToMass
{
    public class AreaProperties
    {
        private Area m_area = null;
        private string areaDocument = "";
        private ElementId areaElementId = ElementId.InvalidElementId;
        private int areaId = -1;
        private string areaUniqueId = "";
        private string areaName = "";
        private string areaNumber = "";
        private string areaTypeName = "";
        private double userHeight = 10;
        private ElementId levelId = ElementId.InvalidElementId;
        private string levelName = "";
        private string designOption = "";
        
        private List<CurveLoop> areaProfile = new List<CurveLoop>();
        private XYZ areaCenterPoint = null;

        private MassProperties linkedMass = null;
        private bool linked = false;
        private bool modifiedHost = false;

        //for datagrid
        private bool isSelected = false;
        private string tooltip = "The linked mass does not exist.";

        public Area AreaElement { get { return m_area; } set { m_area = value; } }
        public string AreaDocument { get { return areaDocument; } set { areaDocument = value; } }
        public ElementId AreaElementId { get { return areaElementId; } set { areaElementId = value; } }
        public int AreaId { get { return areaId; } set { areaId = value; } }
        public string AreaUniqueId { get { return areaUniqueId; } set { areaUniqueId = value; } }
        public string AreaName { get { return areaName; } set { areaName = value; } }
        public string AreaNumber { get { return areaNumber; } set { areaNumber = value; } }
        public string AreaTypeName { get { return areaTypeName; } set { areaTypeName = value; } }
        public double UserHeight { get { return userHeight; } set { userHeight = value; } }
        public ElementId LevelId { get { return levelId; } set { levelId = value; } }
        public string LevelName { get { return levelName; } set { levelName = value; } }
        public string AreaDesignOption { get { return designOption; } set { designOption = value; } }
        public List<CurveLoop> AreaProfile { get { return areaProfile; } set { areaProfile = value; } }
        public XYZ AreaCenterPoint { get { return areaCenterPoint; } set { areaCenterPoint = value; } }
        public MassProperties LinkedMass { get { return linkedMass; } set { linkedMass = value; } }
        public bool Linked { get { return linked; } set { linked = value; } }
        public bool ModifiedHost { get { return modifiedHost; } set { modifiedHost = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
        public string ToolTip { get { return tooltip; } set { tooltip = value; } }
            

        public AreaProperties(Area area)
        {
            m_area = area;
            areaDocument = m_area.Document.Title;
            GetAreaInfo();
        }

        public AreaProperties(AreaProperties ap)
        {
            this.AreaElement = ap.AreaElement;
            this.AreaDocument = ap.AreaDocument;
            this.AreaElementId = ap.AreaElementId;
            this.AreaId = ap.AreaId;
            this.AreaUniqueId = ap.AreaUniqueId;
            this.AreaName = ap.AreaName;
            this.AreaNumber = ap.AreaNumber;
            this.AreaTypeName = ap.AreaTypeName;
            this.UserHeight = ap.UserHeight;
            this.LevelId = ap.LevelId;
            this.LevelName = ap.LevelName;
            this.AreaDesignOption = ap.AreaDesignOption;
            this.AreaProfile = ap.AreaProfile;
            this.AreaCenterPoint = ap.AreaCenterPoint;
            this.LinkedMass = ap.LinkedMass;
            this.Linked = ap.Linked;
            this.ModifiedHost = ap.ModifiedHost;
            this.IsSelected = ap.IsSelected;
            this.ToolTip = ap.ToolTip;
        }

        public void GetAreaInfo()
        {
            try
            {
                areaElementId = m_area.Id;
                areaId = m_area.Id.IntegerValue;
                areaUniqueId = m_area.UniqueId;
                areaName = m_area.Name;
                areaNumber = m_area.Number;

                Parameter areaTypeParam = m_area.get_Parameter(BuiltInParameter.AREA_TYPE);
                if (null != areaTypeParam)
                {
                    areaTypeName = areaTypeParam.AsValueString();
                }

                levelId = m_area.LevelId;
                Level level = m_area.Document.GetElement(levelId) as Level;
                if (null != level)
                {
                    levelName = level.Name;
                }

                if (null != m_area.DesignOption)
                {
                    designOption = m_area.DesignOption.Name;
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
        public void GetAreaProfile()
        {
            try
            {
                SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
                opt.StoreFreeBoundaryFaces = false;
                IList<IList<BoundarySegment>> boundaryListList = m_area.GetBoundarySegments(opt);
                List<CurveLoop> profiles = new List<CurveLoop>();

                foreach (IList<BoundarySegment> boundaryList in boundaryListList)
                {
                    List<Curve> curveList = new List<Curve>();
                    foreach (BoundarySegment segment in boundaryList)
                    {
#if RELEASE2015
                        curveList.Add(segment.Curve);
#elif RELEASE2016
                        curveList.Add(segment.GetCurve());
#endif
                    }
                    CurveLoop curveLoop = CurveLoop.Create(curveList);
                    if (!curveLoop.IsOpen())
                    {
                        areaProfile.Add(curveLoop);
                    }
                }
                if (areaProfile.Count > 0)
                {
                    CalculateCenterPoint();
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void CalculateCenterPoint()
        {
            try
            {
                if (areaProfile.Count > 0)
                {
                    XYZ extrusionDir = new XYZ(0, 0, 1);
                    using (Transaction trans = new Transaction(m_area.Document))
                    {
                        trans.Start("Compute centroid");
                        try
                        {
                            Solid tempSolid = GeometryCreationUtilities.CreateExtrusionGeometry(areaProfile, extrusionDir, 1);
                            if (null != tempSolid)
                            {
                                areaCenterPoint = tempSolid.ComputeCentroid();
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                        }
                        finally
                        {
                            trans.RollBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
