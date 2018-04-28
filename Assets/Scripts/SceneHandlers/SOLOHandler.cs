﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SOLOHandler : MonoBehaviour
{
    public double FireTime;
    private double fireTime;
    private bool isStartWrite = false;
    private bool isUp = false;

    public ChineseInfo[] infos;
    public int currentChinese = 0;
    public Text chinesePinYin;
    public Text chineseContent;
    public Text chineseCurrentPinYin;

    private string[] pinYinArray;
    private string[] contentArray;
    private int currentCharacter = 0;
    private string regResult;

    public ButtonStatusManagr btnManager;


    private void Start()
    {
        RequestInfo();

        fireTime = FireTime;
        ShowHWRModule();
        UpdateChineseInfo(infos[currentChinese]);
    }

    //请求网络数据
    private void RequestInfo()
    {
        infos = new ChineseInfo[4];
        infos[0] = new ChineseInfo("ni hao", "你 好", "用于有礼貌的打招呼或表示与人见面时的问候");
        infos[1] = new ChineseInfo("ke ji", "科 技", "社会上习惯于把科学和技术连在一起，统称为“科技”。实际二者既有密切联系，又有重要区别。科学解决理论问题，技术解决实际问题");
        infos[2] = new ChineseInfo("xian zai", "现 在", "现世,今生;眼前一刹那");
        infos[3] = new ChineseInfo("wei lai", "未 来", "从现在往后的时间");
    }

    private void Update()
    {
        //unity嵌入Android隐藏手写板
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape)) HideHWRModule();


        if (isStartWrite && isUp)
        {
            fireTime -= Time.deltaTime;
            if (fireTime <= 0)
            {
                fireTime = FireTime;
                isStartWrite = false;
                isUp = false;
                HWRRec();
            }
        }
        else
        {
            fireTime = FireTime;
        }
    }

    public void SetTimerStart(string state)
    {
        if ("Down".Equals(state))
        {
            //当按下的时候表示开始写字
            isStartWrite = true;
        }
        else if ("Move".Equals(state))
        {
            isUp = false;
            fireTime = FireTime;
        }
        else if ("Up".Equals(state))
        {
            isUp = true;
        }
    }

    //隐藏手写模块
    public void HideHWRModule()
    {
        AndroidUtil.Call("removeHandWriteBroad");
    }

    //显示手写模块
    private void ShowHWRModule()
    {
        AndroidUtil.Call("addHandWriteBroad");
    }

    //获取Android手写识别后的结果
    public string CallHWRRec()
    {
        return AndroidUtil.Call<string>("hwrRec");
    }

    //手写识别的功能
    public void HWRRec()
    {
        //TODO 限制输入的个数以及实现修改功能
        var results = CallHWRRec();
        AndroidUtil.Log("识别到的结果:" + results);
        if (results != null && results.Length >= 1)
        {
            var resultArr = results.Split(';');
            if (resultArr.Length > 0)
            {
                AddCharacter(resultArr[0]);
                currentCharacter++;
                if (currentCharacter == contentArray.Length)
                    btnManager.SetAllInteractable();
                else
                    SetNewPinYin();
            }
        }
    }

    //用于Unity下调试手写识别结果反馈
    public void TestHWRRec(String results)
    {
        AndroidUtil.Log("识别到的结果:" + results);
        if (results != null && results.Length >= 1)
        {
            AddCharacter(results.Substring(0, 1));
            currentCharacter++;
            if (currentCharacter == contentArray.Length)
                btnManager.SetAllInteractable();
            else
                SetNewPinYin();
        }
    }

    public void ResultJudge()
    {
        if (!chineseContent.text.Equals(infos[currentChinese].Content))
        {
            //TODO 错误效果
            AndroidUtil.Toast("输入错误!!!\n" + "实际: " + chineseContent.text + "\n输入: " +
                              infos[currentChinese].Content);
        }
        else
        {
            //TODO 攻击效果
            AndroidUtil.Toast("攻击效果!!!");
        }

        currentChinese++;
        if (currentChinese == infos.Length)
        {
            //TODO 游戏结束
            AndroidUtil.Toast("游戏结束");
            HideHWRModule();
        }
        else
        {
            UpdateChineseInfo(infos[currentChinese]);
        }
    }

    public void UpdateChineseInfo(ChineseInfo chinese)
    {
        currentCharacter = 0;
        pinYinArray = chinese.Pinyin.Split(' ');
        contentArray = chinese.Content.Split(' ');
        AndroidUtil.Log("拼音: " + GeneralTools.printArray(pinYinArray));
        AndroidUtil.Log("内容: " + GeneralTools.printArray(contentArray));

        chinesePinYin.text = chinese.Pinyin;
        chineseContent.text = "";
        chineseCurrentPinYin.text = pinYinArray[currentCharacter];
    }

    public void AddCharacter(string s)
    {
        if (chineseContent.text.Length > 0)
            chineseContent.text += " " + s;
        else
            chineseContent.text += s;
    }

    public void SetNewPinYin()
    {
        chineseCurrentPinYin.text = pinYinArray[currentCharacter];
    }

    public void ShowDetialContent()
    {
        AndroidUtil.Toast(infos[currentChinese].Detail, 0, 0);
    }
}