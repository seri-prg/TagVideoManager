

// ボリューム保存セットアップ
function SetupKeepVolume(video)
{
    video.addEventListener("volumechange", (event) => 
    {
        localStorage.setItem('volume', event.target.volume);
    });
    
    // 以前のボリュームを復帰
    PullVolume(video);
}


// 以前のボリュームを復帰
function PullVolume(video)
{
    var volume = localStorage.getItem('volume');
    if(volume != null)
    {
        video.volume = volume;
    }
}



// ボタンを押されたら現在の動画の位置を取得
function SetupBtnPlayPoint(buttonName, callback)
{
    var buttonElem = document.querySelectorAll(buttonName);
    buttonElem.forEach((i) => 
    {
        if (i.getAttribute("add_play_point")!=null)
            return;
    
        i.setAttribute("add_play_point", "true");

        i.addEventListener("click", (elem)=>
        {
            callback(elem.target);
        });
    });
}


// 開始ポイント削除ボタンの更新
function UpdatePlayPointRemoveBtn(buttonName, opCode, text, removeId)
{
    var remove = document.getElementById(buttonName);
    remove.textContent = text;
    remove.setAttribute("remove_id", removeId); // opCodeによって意味が変わる
    remove.setAttribute("op_code", opCode); // r=タグリンクId ra=タグId
    remove.style.display = "block";  // 表示
}


function SetupPlayPointRemoveBtn(buttonName, callback)
{
    var remove = HideElem(buttonName);
/*
    var remove = document.getElementById(buttonName);
    remove.style.display = "none";  // 最初は非表示
*/
    CheckDuplicate(remove, "remove_tag_link", ()=>
    {
       remove.addEventListener("click", (elem)=>
       {
            var opCode = elem.target.getAttribute("op_code");
            var removeId = elem.target.getAttribute("remove_id");
            callback(opCode, removeId);
            elem.target.style.display = "none";  // 
        });
    });
}

// 非表示
function HideElem(targetId)
{
    var hide = document.getElementById(targetId);
    hide.style.display = "none";  // 最初は非表示
    return hide;
}


