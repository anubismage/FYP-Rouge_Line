using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager { get; private set; }

    public HealthSystem playerHealth = new HealthSystem(100, 100);

    public GameObject Generator;
    private float lerpTimer;
    public float chipSpeed = 2f;
    public Image frontHealthBar;
    public Image backHealthBar;
    public Text text;
    public Text seedtext;
    public Text pointsText;
    public GameObject panel;
    public GameObject DeathText;
    public GameObject EndPanel;
    public Text EndText;
    public bool playerloaded = false;
    public DungeonGenerator gen;

    public int Difficulty1;
    public int Difficulty2;
    public int Difficulty3;
    public int Points;
    [SerializeField] private bool UsePreDetermindedDifficulty = true;
   // private List<(float,int,int)> ValuesToSave = new List<(float,int,int)>(); //Time, Point ,Health 
    string filename = "";

    [System.Serializable]
    public class TestValues
    {
        public float Time;
        public int Points;
        public int Health;

        public TestValues(float t,int p, int h)
        {
            Time = t;
            Points = p;
            Health = h;
        }
    }

    List<TestValues> ValuesToSave = new List<TestValues>();

    void Awake()
    {
        if (gameManager != null && gameManager != this)
        {
            Destroy(this);
        }
        else
        {
            gameManager = this;
        }
    }

    private void Start()
    {
        //string datetime = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
        filename = Application.dataPath + "/Results.csv";       
    }

    // Update is called once per frame
    void Update()
    {
        text.text = playerHealth.Health.ToString();
        seedtext.text = Generator.GetComponent<DungeonGenerator>().seedNumber.ToString();
        //seedtext.text = Time.fixedTime.ToString();
        if (playerloaded) {
            panel.SetActive(true);
            UpdateHealthUI(); 
        }
        
        if(playerHealth.Health < 1)
        {
            DeathText.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void UpdateHealthUI()
    {

        float fillF = frontHealthBar.fillAmount;
        float fillB = backHealthBar.fillAmount;
        float hFraction = (float)playerHealth.Health / playerHealth.MaxHealth;
        
        if (fillB > hFraction)
        {
            frontHealthBar.fillAmount = hFraction;
            backHealthBar.color = Color.red;
            lerpTimer += Time.deltaTime;
            float percentComplete = (lerpTimer / chipSpeed)*2;
            backHealthBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }
        if (fillF < hFraction)
        {
            backHealthBar.fillAmount = hFraction;
            backHealthBar.color = Color.green;
            lerpTimer += Time.deltaTime;
            float percentComplete = (lerpTimer / chipSpeed)*2;
            frontHealthBar.fillAmount = Mathf.Lerp(fillF, backHealthBar.fillAmount, percentComplete);
        }
    }

    public void TakeDamage(int dmg)
    {
        playerHealth.Damage(dmg);
        lerpTimer = 0f;

    }

    public void RestoreHealth(int heal)
    {
        playerHealth.Heal(heal);
        lerpTimer = 0f;

    }

    public void AddPoints(int p)
    {
        Points = Points + p;
        pointsText.text = Points.ToString();
        ValuesToSave.Add(new TestValues(Time.fixedTime, Points, playerHealth.Health));

    }

    public int GetPoints()
    {
        return Points;
    }

    public void WriteCSV()
    {
        if(ValuesToSave.Count > 0)
        {
            TextWriter tw = new StreamWriter(filename, false);
            tw.WriteLine("Seed: "+gen.seedNumber.ToString()+" TimeElapsed"+","+ "PointsGained"+","+"PlayerHealth");
            tw.Close();

            tw = new StreamWriter(filename, true);

            foreach(TestValues values in ValuesToSave)
            {
                tw.WriteLine(values.Time + ","+values.Points+","+values.Health);
            }
            tw.Close();
        }
    }

    public bool getPDiff()
    {
        return UsePreDetermindedDifficulty;
    }

    public void EndLevel()
    {
        EndPanel.SetActive(true);
        EndText.text = Time.fixedTime.ToString();

        Time.timeScale = 0;
    }

    void OnApplicationQuit()
    {
        ValuesToSave.Add(new TestValues(Time.fixedTime, Points, playerHealth.Health));
        Debug.Log("Application ending after " + Time.time + " seconds");
        WriteCSV();
    }
}
