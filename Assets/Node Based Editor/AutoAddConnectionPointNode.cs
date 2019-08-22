using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditor;
using System;
using UnityEditor;
using System.IO;

public class AutoAddConnectionPointNode : Node
{
    protected int minInPointNum = 1;
    protected int maxInPointNum = -1;
    protected int minOutPointNum = 1;
    protected int maxOutPointNum = -1;

    #region Constructors
    public AutoAddConnectionPointNode(
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle,
            Func<ConnectionPoint, ConnectionPoint> onClickInPoint,
            Func<ConnectionPoint, ConnectionPoint> onClickOutPoint,
            Action<ConnectionPoint> onRemoveConnectionPoint)
        : base(
            nodeStyle,
            selectedStyle,
            inPointStyle,
            outPointStyle,
            onClickInPoint,
            onClickOutPoint,
            onRemoveConnectionPoint)
    {
        AddInPoint();
        AddOutPoint();
    }

    public AutoAddConnectionPointNode(
            int id,
            Vector2 position,
            float width,
            float height,
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle,
            Func<ConnectionPoint, ConnectionPoint> onClickInPoint,
            Func<ConnectionPoint, ConnectionPoint> onClickOutPoint,
            Action<ConnectionPoint> onRemoveConnectionPoint)
        : base(
            id,
            position,
            width,
            height,
            nodeStyle,
            selectedStyle,
            inPointStyle,
            outPointStyle,
            onClickInPoint,
            onClickOutPoint,
            onRemoveConnectionPoint)
    {
        AddInPoint();
        AddOutPoint();
    }

    public AutoAddConnectionPointNode(
            int minInPointNum,
            int minOutPointNum,
            int maxInPointNum,
            int maxOutPointNum,
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle,
            Func<ConnectionPoint, ConnectionPoint> onClickInPoint,
            Func<ConnectionPoint, ConnectionPoint> onClickOutPoint,
            Action<ConnectionPoint> onRemoveConnectionPoint)
        : base(
            nodeStyle,
            selectedStyle,
            inPointStyle,
            outPointStyle,
            onClickInPoint,
            onClickOutPoint,
            onRemoveConnectionPoint)
    {
        this.minInPointNum = minInPointNum;
        this.minOutPointNum = minOutPointNum;
        this.maxInPointNum = maxInPointNum;
        this.maxOutPointNum = maxOutPointNum;
        AddInPoint();
        AddOutPoint();
    }

    public AutoAddConnectionPointNode(
            int id,
            Vector2 position,
            float width,
            float height,
            int minInPointNum,
            int minOutPointNum,
            int maxInPointNum,
            int maxOutPointNum,
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle,
            Func<ConnectionPoint, ConnectionPoint> onClickInPoint,
            Func<ConnectionPoint, ConnectionPoint> onClickOutPoint,
            Action<ConnectionPoint> onRemoveConnectionPoint)
        : base(
            id,
            position,
            width,
            height,
            nodeStyle,
            selectedStyle,
            inPointStyle,
            outPointStyle,
            onClickInPoint,
            onClickOutPoint,
            onRemoveConnectionPoint)
    {
        this.minInPointNum = minInPointNum;
        this.minOutPointNum = minOutPointNum;
        this.maxInPointNum = maxInPointNum;
        this.maxOutPointNum = maxOutPointNum;
        if(minInPointNum > 0)
            AddInPoint();
        if(minOutPointNum > 0)
            AddOutPoint();
    }

    #endregion

    public override void OnCreateConnectionPoint(ConnectionPoint point)
    {
        point.MaxConnectionNum = 1;
    }

    public override void AddInPoint(int count = 1)
    {
        if(maxInPointNum < 0 || inPoints.Count < maxInPointNum)
            base.AddInPoint(count);
    }

    public override void AddOutPoint(int count = 1)
    {
        if(maxOutPointNum < 0 || outPoints.Count < maxOutPointNum)
            base.AddOutPoint(count);
    }

    protected override bool RemoveConnectionPoint(ConnectionPoint point)
    {
        if ((point.type == ConnectionPointType.In && inPoints.Count > minInPointNum) ||
            (point.type == ConnectionPointType.Out && outPoints.Count > minOutPointNum))
        {
            return base.RemoveConnectionPoint(point);
        }
        return false;
    }

    protected override void OnConnectConnectionPoint(ConnectionPoint own, ConnectionPoint other)
    {
        if (own.type == ConnectionPointType.In)
            AddInPoint();
        else if (own.type == ConnectionPointType.Out)
            AddOutPoint();
    }

    protected override void OnDisconnectConnectionPoint(ConnectionPoint own, ConnectionPoint other)
    {
        if (own.opposites.Count <= 0)
        {
            if (own.type == ConnectionPointType.In && inPoints.Count > 1)
            {
                inPoints.Remove(own);
            }
            else if (own.type == ConnectionPointType.Out && outPoints.Count > 1)
            {
                outPoints.Remove(own);
            }

            LayoutConnectionPoints();
        }
    }

    protected override void AddGenericItem(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        menu.AddItem(new GUIContent("Copy"), false, () => onCopyNode(this));
    }

    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);
        writer.Write(minInPointNum);
        writer.Write(maxInPointNum);
        writer.Write(minOutPointNum);
        writer.Write(maxOutPointNum);
    }

    public override void Load(BinaryReader reader, int header)
    {
        base.Load(reader, header);
        minInPointNum = reader.ReadInt32();
        maxInPointNum = reader.ReadInt32();
        minOutPointNum = reader.ReadInt32();
        maxOutPointNum = reader.ReadInt32();
    }
}
