using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class GridConversionLogic : MonoBehaviour
{
     private static int gridHeight = 1;
     private static int gridWidth = 1;
    [SerializeField] private GameObject viewPlane;
    [SerializeField] private ViewCameraHandler viewHandler;
    [SerializeField] private OrthoCameraHandler orthoHandler;
    [SerializeField] private Camera orthoBrushCam;
    private float planeScale = 12f;
    private int tileSize;
   
    public struct gridCell
    {
       public int column;
       public int row;
       public bool isOccupied;
       public GameObject viewPlane;
    }
    private gridCell[,] Grids = new gridCell[gridHeight,gridWidth];
    void Awake()
    {
        if (viewPlane != null)
        {
            planeScale = viewPlane.transform.localScale.x;
        }
        tileSize = (int)(planeScale * 10);

        InitializeGrid();
    }
    void Start()
    {
        
        if (orthoHandler != null)
        {
            orthoHandler.IncreaseSize(gridHeight * tileSize);
            float newX = (gridWidth-1) * tileSize / 2.0f;
            float newY = (gridHeight-1) * tileSize / 2.0f;
            Vector2 newCoords = new Vector2(newX, newY);
            orthoHandler.ReAlignCamera(newCoords);
        }
        if (viewHandler != null)
        {
            viewHandler.MoveCameraFurther(gridHeight * tileSize * 0.5f);
            float newX = (gridWidth-1) * tileSize / 2.0f;
            float newY = (gridHeight-1) * tileSize / 2.0f;
            Vector2 newCoords = new Vector2(newX, newY);
            viewHandler.ReAlignCamera(newCoords);
        }

        InitializePlaneTex();
    }
    public void InitializeGrid()
    {    
        for(int i = 0; i < gridHeight; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                gridCell cell;
                cell.isOccupied = false;
                cell.column = i * tileSize;
                cell.row = j * tileSize;
                Vector3 spawnPos = new Vector3(cell.column, 0, cell.row);
                cell.viewPlane = Instantiate(viewPlane, spawnPos, viewPlane.transform.rotation);
                Grids[i,j]= cell;
            }            
        }
        
    }
    private void InitializePlaneTex()
    {

        Texture2D initialTex = BuildEmptyTex();
        if (initialTex == null)
        {
            Debug.LogError("Initial texture is null, check BuildEmptyTex()");
            return;
        }

        for(int i=0;i<gridHeight; i++)
        {
            for(int j=0;j<gridWidth; j++)
            {
                if (Grids[i,j].viewPlane != null)
                {
                    Renderer planeR = Grids[i, j].viewPlane.GetComponent<Renderer>();
                    planeR.material.SetTexture("_BaseMap", initialTex);
                }
            }
        }
    }
    private Texture2D BuildEmptyTex()
    {
        RenderTexture currTex = RenderTexture.active;
        if (orthoBrushCam == null)
        {
            Debug.LogError("Assign Brush Trail camera or dummy camera to the grid");
            return null;
        }
        orthoBrushCam.Render();

        RenderTexture rt = orthoBrushCam.targetTexture;
        RenderTexture.active = rt;
        Texture2D initialTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        Rect rect = new Rect(0, 0, rt.width, rt.height);
        initialTex.ReadPixels(rect, 0, 0);
        initialTex.Apply();
        RenderTexture.active = currTex;
        return initialTex;
    }
    private Vector2 getNextCell()
    {
        Vector2 nextCell = new Vector2(-1,-1);
        for(int i = 0; i < gridHeight; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                if (!Grids[i,j].isOccupied)
                {
                    Grids[i, j].isOccupied = true;
                    return new Vector2(i, j);
                }
            }
        }
        //Add new columns/rows
        RebuildGrid();
        //return getNextCell()
        nextCell = getNextCell();
        return nextCell;
    }
   
    public void ApplyTexture(Texture2D tex)
    {
        Vector2 cellIndices = getNextCell();
        int col = (int)cellIndices.x;
        int row = (int)cellIndices.y;
        if(col <0 || row < 0)
        {
            Debug.LogError("Failed to expand grid");
            return;
        }
        
        //Vector3 spawnPos = new Vector3(Grids[col,row].column, 0, Grids[col,row].row);
        //Grids[col,row].viewPlane = Instantiate(viewPlane,spawnPos, viewPlane.transform.rotation);
        
        Renderer cellRenderer = Grids[col, row].viewPlane.GetComponent<Renderer>();
        if (cellRenderer != null)
            cellRenderer.material.SetTexture("_BaseMap", tex);
        else
            Debug.Log("Cell Renderer for chosen plane is null");
    }

    private void RebuildGrid()
    {
        gridHeight++;
        gridWidth++;
        gridCell[,] OldGrid = Grids;
        Grids = new gridCell[gridHeight, gridWidth];

        int oldHeight = gridHeight - 1;
        int oldWidth = gridWidth - 1;
        Texture2D initTex = BuildEmptyTex();
        if(initTex == null)
        {
            Debug.LogError("Initial tex is null");
            return;
        }

        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                gridCell cell;
                cell.isOccupied = false;
                cell.column = i * tileSize;
                cell.row = j * tileSize;
                cell.viewPlane = null;
                Vector3 spawnPos = new Vector3(cell.column, 0, cell.row);
                if (i < oldHeight)
                {
                    if (j < oldWidth)
                    {
                        cell.viewPlane = OldGrid[i, j].viewPlane;
                        cell.isOccupied = OldGrid[i, j].isOccupied;
                    }
                    else
                    {
                        cell.viewPlane = Instantiate(viewPlane, spawnPos, viewPlane.transform.rotation);
                        Renderer planeR = cell.viewPlane.GetComponent<Renderer>();
                        planeR.material.SetTexture("_BaseMap", initTex);
                    }
                }
                else
                {
                    cell.viewPlane = Instantiate(viewPlane, spawnPos, viewPlane.transform.rotation);
                    Renderer planeR = cell.viewPlane.GetComponent<Renderer>();
                    planeR.material.SetTexture("_BaseMap", initTex);

                }
                Grids[i, j] = cell;
            }
        }
        if (orthoHandler != null) { 
            orthoHandler.IncreaseSize(gridHeight * tileSize);
            float newX = (gridWidth-1) * tileSize / 2.0f;
            float newY = (gridHeight-1) * tileSize / 2.0f;
            Vector2 newCoords = new Vector2(newX, newY);
            orthoHandler.ReAlignCamera(newCoords);
        }
        if (viewHandler != null) {
            viewHandler.MoveCameraFurther(gridHeight * tileSize * 0.5f);
            float newX = (gridWidth-1) * tileSize / 2.0f;
            float newY = (gridHeight-1) * tileSize / 2.0f;
            Vector2 newCoords = new Vector2(newX, newY);
            viewHandler.ReAlignCamera(newCoords);
        }

    }

}
