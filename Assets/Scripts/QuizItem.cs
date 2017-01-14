using UnityEngine;
using System.Collections;
using StageNine;

public class QuizItem : CameraDragger
{
    //enums

    //subclasses

    //consts and static data

    //public data

    //private data
    TriviaPair data;

    //public properties

    //methods
    #region public methods

    public override void GetTap()
    {
        if(GameFieldManager.singleton.canTapQuizItems)
        {
            if(GameFieldManager.singleton.QuizCorrectAnswer(data))
            {
                // right
            }
            else
            {
                // wrong
            }
        }
    }

    #endregion

    #region private methods

    #endregion

    #region monobehaviors

    #endregion

}
