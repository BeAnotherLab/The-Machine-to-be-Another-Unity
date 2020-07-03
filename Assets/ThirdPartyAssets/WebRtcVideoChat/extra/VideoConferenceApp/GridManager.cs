using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Helper to position the video images of the conference calls.
/// </summary>
[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{

    private int mKnownItems = 0;
    private GridLayoutGroup mGrid;
    private RectTransform mTransform;


	void Update ()
    {
        if (mGrid == null)
            mGrid = GetComponent<GridLayoutGroup>();
        if (mTransform == null)
            mTransform = GetComponent<RectTransform>();

        if (mKnownItems != this.transform.childCount && mGrid != null && mTransform != null)
        {
            mKnownItems = this.transform.childCount;
            Refresh();
        }
	}
    private void Refresh()
    {
        int sq = 1;
        if(mKnownItems > 0)
        {
            sq = Mathf.CeilToInt(Mathf.Sqrt(mKnownItems));
        }
        int rows = sq;



        float availableWidth = mTransform.rect.size.x - mGrid.padding.left - mGrid.padding.right;
        float availableHeight = mTransform.rect.size.y - mGrid.padding.top - mGrid.padding.bottom;

        Vector2 cellSize = new Vector2();
        cellSize.x = availableWidth / rows - mGrid.spacing.x;
        cellSize .y = availableHeight / rows - mGrid.spacing.y;

        mGrid.cellSize = cellSize;


    }



}
