//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EnemySelector : MonoBehaviour
//{
//    public enum CursorStatus
//    {
//        selected,
//        deselected
//    }
//    [SerializeField] SpriteRenderer cursor;
//    [SerializeField] CursorStatus currentCursorStatus;
//    public Moveset moveset;
//    // Start is called before the first frame update
//    void Start()
//    {
//        moveset = GameObject.FindGameObjectWithTag("Player").GetComponent<Moveset>();
//    }

//    void OnMouseOver()
//    {
//        if (currentCursorStatus == CursorStatus.deselected)
//        {
//            cursor.color = new Color(0, 0, 0, 1);
//        }
//    }
//    void OnMouseDown()
//    {
//        if (currentCursorStatus == CursorStatus.deselected && moveset.player.currentTarget == null)
//        {
//            cursor.color = Color.green;
//            currentCursorStatus = CursorStatus.selected;
//            moveset.SelectCurrentTarget(this.gameObject);
//            Debug.Log(this.gameObject);
//        }
//        else if (currentCursorStatus == CursorStatus.selected)
//        {
//            cursor.color = new Color(0, 0, 0, 0);
//            currentCursorStatus = CursorStatus.deselected;
//            moveset.SelectCurrentTarget(null);
//            Debug.Log("Deselected target");
//        }
//    }
//    void OnMouseExit()
//    {
//        if (currentCursorStatus == CursorStatus.deselected)
//        {
//            cursor.color = new Color(0, 0, 0, 0);
//        }
//    }
//}
