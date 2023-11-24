/*
	  var v = document.getElementById('video');
	  var state = document.getElementById('state');
	  //ロード開始
	  v.addEventListener('loadedmetadata', function() {
		state.textContent = 'ロードを開始しました';
	  })
	  //読み込み完了
	  v.addEventListener('loadeddata', function() {
		state.textContent = '読み込み完了しました';
	  })
	  //再生可能
	  v.addEventListener('canplay', function() {
		state.textContent = '再生可能です';
	  })
	  //再生中
	  v.addEventListener('playing', function() {
		state.textContent = '再生中です';
		})


*/





// ツリービュー用設定
function SetupTreeView(className, nestedClass, callback)
{
    var tagList = document.querySelectorAll(className);

    // ノードを展開する
    function childActive(target, nestedClass)
    {
        target.parentElement.querySelector(nestedClass).classList.toggle("active");
        target.classList.toggle("caret-down");
    }

    tagList.forEach((i) =>
    {
        childActive(i, nestedClass);
        CheckDuplicate(i, "treeview_nested", ()=>
        {
            i.addEventListener("click", function()
            {
                childActive(this, nestedClass);
                callback(this);
            });
        });
    });
};



// ボタンを押されたら任意の位置から動画再生する処理登録
function SetupPlayVideo(buttonName, target, startTimeAttr, callback)
{
    var buttonElem = document.querySelectorAll(buttonName);
    buttonElem.forEach((i) => 
    {
        CheckDuplicate(i, "media_play", ()=>
        {
            i.addEventListener("click", (elem)=>
            {
                var time = elem.target.getAttribute(startTimeAttr);
                if (time == null)
                {
                    time = 0;
                }
    
                var v = document.getElementById(target);
                v.currentTime = time;

                callback(elem.target, time);
               // v.play();
            });
        });
    });
}


// ボリューム保存セットアップ
function SetupKeepVolume(videoName)
{
    var video = document.getElementById(videoName);
    video.addEventListener("volumechange", (event) => 
    {
        localStorage.setItem('volume', event.target.volume);
    });
    
    // 以前のボリュームを復帰
    var volume = localStorage.getItem('volume');
    if(volume)
    {
        video.volume = volume;
    }
}



// ボタンを押されたら現在の動画の位置を取得
function SetupBtnPlayPoint(buttonName, target, callback)
{
    var buttonElem = document.querySelectorAll(buttonName);
    buttonElem.forEach((i) => 
    {
        if (i.getAttribute("add_play_point")!=null)
            return;
    
        i.setAttribute("add_play_point", "true");

        i.addEventListener("click", (elem)=>
        {
            var v = document.getElementById(target);
            callback(elem.target, v.currentTime);
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


