using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QuestionNode : ITreeNode
{
    private readonly Func<bool> _question;
    private readonly ITreeNode _trueNode;
    private readonly ITreeNode _falseNode;

    public QuestionNode(Func <bool> question, ITreeNode trueNode, ITreeNode falseNode)
    {
        _question = question;
        _trueNode = trueNode;
        _falseNode = falseNode;
    }

    public void Execute()
    {
        if (_question())
        {
            _trueNode.Execute();
        }
        else
        {
            _falseNode.Execute();
        }
    }
}

