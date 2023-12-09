/*
	汎用処理

*/


// サーバにリクエストを送りjsonで取得
function RequestJson(query, callback)
{
	fetch(query)
		.then((response) => response.json())
		.then((json) => callback(json)); 
}


// リクエスト投げて終了
function callHtml(query)
{
	fetch(query);
}




// 重複チェック
function CheckDuplicate(target, name, callback)
{
    if (target.getAttribute(name)!=null)
        return;

    target.setAttribute(name, "true");
    callback();
}


// 確認ダイアログでOKなら処理
function SetupConfirm(elemName, msg, callback)
{
    var btn = document.getElementById(elemName);
    btn.addEventListener('click', ()=> 
    {
        if(window.confirm(msg))
        {
            callback();
        }
    });
}

// 子要素を全て削除する
function RemoveChildAll(target)
{
	while(target.firstChild)
	{
		target.removeChild(target.firstChild);
	}
}


// 一定時間メッセージを表示してフェードアウト
function ShowMsg(targetId, msg, timeSpan)
{
	const smsg = document.getElementById(targetId);
	smsg.classList.remove("fade_out");
	smsg.innerText = msg;
	window.setTimeout(()=>
	{
		smsg.classList.toggle("fade_out");
	}, timeSpan);
}

// 表示のon/off切り替え
function toggleVisible(elemId)
{
	const elem = document.getElementById(elemId);
	if (elem.style.visibility == 'visible')
	{
		elem.style.visibility = 'hidden';
	}
	else
	{
		elem.style.visibility = 'visible';
	}
}

// モバイル端末か否か
function IsMobile() 
{
	var ua = navigator.userAgent;
	if (ua.indexOf('iPhone') > 0 || 
		ua.indexOf('iPad') > 0 || 
		ua.indexOf('Android') > 0 || 
		ua.indexOf('Mobile') > 0 )
		return true;
}

