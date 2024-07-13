﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Matryx;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

public class TournamentsMenu : MonoBehaviour
{
    public static TournamentsMenu Instance { get; private set; }
    public TournamentMenuState state;

    private CalcManager calcManager;
    private MultiSelectFlexPanel tournamentsPanel;
    [SerializeField]
    public MyAccountButton myAccountButton;
    [SerializeField]
    TMPro.TextMeshPro selectTournamentText;
    [SerializeField]
    public UnlockAccountButton unlockButton;
    [SerializeField]
    private CreateTournamentButton createTournamentButton;
    [SerializeField]
    private MyCommitsButton myCommitsButton;
    [SerializeField]
    private MyTournamentsButton myTournamentsButton;
    [SerializeField]
    private TournamentMenu tournamentMenu;
    [SerializeField]
    private CreateSubmissionMenu submitMenu;
    [SerializeField]
    private TMPro.TextMeshPro tournamentListText;

    private Scroll scroll;
    JoyStickAggregator joyStickAggregator;
    FlexMenu flexMenu;

    private List<MatryxTournament> tournaments = new List<MatryxTournament>();

    public enum TournamentMenuState
    {
        AccountUnlockRequired,
        WaitingForUnlock,
        Unlocked
    }

    internal class TournamentButtonResponder : FlexMenu.FlexMenuResponder
    {
        public FlexMenu menu;
        TournamentsMenu tournamentMenu;
        internal TournamentButtonResponder(TournamentsMenu tournamentMenu, FlexMenu menu)
        {
            this.menu = menu;
            this.tournamentMenu = tournamentMenu;
        }

        public void Flex_ActionStart(string name, FlexActionableComponent sender, GameObject collider)
        {
            tournamentMenu.HandleInput(sender.gameObject);
        }

        public void Flex_ActionEnd(string name, FlexActionableComponent sender, GameObject collider) { }
    }

    private void HandleInput(GameObject source)
    {
        if (source.name == "Load_Button")
        {
            ReloadTournaments();
        }
        else if (source.GetComponent<TournamentContainer>())
        {
            string name = source.name;

            MatryxTournament tournament = source.GetComponent<TournamentContainer>().tournament;
            tournamentMenu.SetTournament(tournament);
            submitMenu.SetTournament(tournament);
            tournamentMenu.gameObject.GetComponent<AnimationHandler>().OpenMenu();
        }
    }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Initialize(CalcManager calcManager)
    {
        this.calcManager = calcManager;

        scroll = GetComponentInChildren<Scroll>(true);
        flexMenu = GetComponent<FlexMenu>();
        TournamentButtonResponder responder = new TournamentButtonResponder(this, flexMenu);
        flexMenu.RegisterResponder(responder);
        tournamentsPanel = GetComponentInChildren<MultiSelectFlexPanel>().Initialize();
        joyStickAggregator = scroll.GetComponent<JoyStickAggregator>();
    }

    public void Prepare()
    {
        createTournamentButton.Awake();
        myCommitsButton.Awake();
        myTournamentsButton.Awake();

        if (NetworkSettings.declinedAccountUnlock == null )
        {
            SetState(TournamentMenuState.AccountUnlockRequired);
        }
        else if(NetworkSettings.declinedAccountUnlock.Value)
        {
            SetState(TournamentMenuState.WaitingForUnlock);
        }
        else
        {
            SetState(TournamentMenuState.Unlocked);
        }
    }

    public static void SetState(TournamentMenuState state)
    {
        if (Instance == null) return;
        Instance.state = state;

        switch (Instance.state)
        {
            case TournamentMenuState.AccountUnlockRequired:
                Instance.ClearTournaments();
                Instance.selectTournamentText.text = "Account Unlock Required";
                Instance.unlockButton.transform.parent.gameObject.SetActive(true);
                Instance.unlockButton.SetButtonToUnlock();
                Instance.tournamentListText.gameObject.SetActive(false);
                Instance.myAccountButton.transform.parent.gameObject.SetActive(false);
                CreateTournamentButton.Instance.transform.parent.gameObject.SetActive(false);
                MyCommitsButton.Instance.transform.parent.gameObject.SetActive(false);
                MyTournamentsButton.Instance.transform.parent.gameObject.SetActive(false);
                break;
            case TournamentMenuState.WaitingForUnlock:
                Instance.ClearTournaments();
                Instance.selectTournamentText.text = "Lift Headset to Unlock Account";
                Instance.unlockButton.transform.parent.gameObject.SetActive(true);
                Instance.unlockButton.SetButtonToCancel();
                Instance.tournamentListText.gameObject.SetActive(false);
                Instance.myAccountButton.transform.parent.gameObject.SetActive(false);
                CreateTournamentButton.Instance.transform.parent.gameObject.SetActive(false);
                MyCommitsButton.Instance.transform.parent.gameObject.SetActive(false);
                MyTournamentsButton.Instance.transform.parent.gameObject.SetActive(false);
                break;
            case TournamentMenuState.Unlocked:
                Instance.selectTournamentText.text = "";
                Instance.unlockButton.transform.parent.gameObject.SetActive(false);
                Instance.tournamentListText.gameObject.SetActive(true);
                Instance.LoadTournaments(0);
                Instance.myAccountButton.transform.parent.gameObject.SetActive(true);
                CreateTournamentButton.Instance.transform.parent.gameObject.SetActive(true);
                MyCommitsButton.Instance.transform.parent.gameObject.SetActive(true);
                MyTournamentsButton.Instance.transform.parent.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    int page = 0;
    bool loaded = false;
    bool loading = false;
    public bool LoadTournaments(int thePage, float waitTime = 0)
    {
        if ((loaded | loading ) == true) return false;
        loading = true;
        tournamentListText.text = "Loading Tournaments...";
        tournamentListText.fontStyle = TMPro.FontStyles.Normal;
        ClearTournaments();
        MatryxCortex.RunGetTournaments(thePage, waitTime, ProcessTournaments, ShowError);
        return true;
    }

    /// <summary>
    /// Loads the next page of tournaments.
    /// </summary>
    public void ReloadTournaments(float waitTime = 0)
    {
        loaded = false;
        loading = false;
        if (LoadTournaments(page + 1, waitTime))
        {
            page++;
        }
    }

    /// <summary>
    /// Clears the list of tournaments.
    /// </summary>
    public void ClearTournaments()
    {
        page = 0;
        tournaments.Clear();
        scroll.clear();
    }

    private void ProcessTournaments(object results)
    {
        tournaments = (List<MatryxTournament>)results;
        tournamentListText.text = "Open Tournaments";
        tournamentListText.fontStyle = TMPro.FontStyles.Underline;
        DisplayTournaments(tournaments);
        loaded = true;
    }

    private void ShowError(object results)
    {
        tournamentListText.text = "Unable to Load Any Tournaments";
        loaded = false;
    }

    GameObject loadButton;
    private void DisplayTournaments(List<MatryxTournament> _tournaments)
    {
        foreach (MatryxTournament tournament in _tournaments)
        {
            GameObject button = createButton(tournament);
            button.SetActive(false);
            tournamentsPanel.AddAction(button.GetComponent<FlexButtonComponent>());
        }

        loadButton = createLoadButton();
    }

    private GameObject createLoadButton()
    {
        GameObject button = Instantiate(Resources.Load("Tournament_Cell", typeof(GameObject))) as GameObject;
        button.transform.SetParent(tournamentsPanel.transform);
        button.transform.localScale = Vector3.one;
        button.transform.position = new Vector3(-500f, -500f, -500f);

        button.name = "Load_Button";
        var text = button.transform.Find("Text").GetComponent<TMPro.TextMeshPro>();
        text.text = "Reload Tournaments";
        text.fontStyle = TMPro.FontStyles.Bold;
        text.alignment = TMPro.TextAlignmentOptions.Center;

        TMPro.TextMeshPro matryxBountyTMP = button.transform.Find("MTX_Amount").GetComponent<TMPro.TextMeshPro>();
        matryxBountyTMP.text = "";

        scroll.addObject(button.transform);
        joyStickAggregator.AddForwarder(button.GetComponentInChildren<JoyStickForwarder>());

        // Add this action to the panel
        tournamentsPanel.AddAction(button.GetComponent<FlexButtonComponent>());

        return button;
    }

    private void removeLoadButton()
    {
        if (loadButton != null)
        {
            List<Transform> loadButtonTransform = new List<Transform>();
            loadButtonTransform.Add(loadButton.transform);
            scroll.deleteObjects(new List<Transform>(loadButtonTransform));

            Destroy(loadButton);
        }
    }

    private GameObject createButton(MatryxTournament tournament)
    {
        GameObject button = Instantiate(Resources.Load("Tournament_Cell", typeof(GameObject))) as GameObject;
        button.transform.SetParent(tournamentsPanel.transform);
        button.transform.localScale = Vector3.one;

        button.name = tournament.title;
        button.GetComponent<TournamentContainer>().tournament = tournament;

        button.transform.Find("Text").GetComponent<TMPro.TextMeshPro>().text = tournament.title;

        TMPro.TextMeshPro matryxBountyTMP = button.transform.Find("MTX_Amount").GetComponent<TMPro.TextMeshPro>();
        matryxBountyTMP.text = tournament.Bounty + " " + matryxBountyTMP.text;

        scroll.addObject(button.transform);
        joyStickAggregator.AddForwarder(button.GetComponentInChildren<JoyStickForwarder>());

        return button;
    }
}
