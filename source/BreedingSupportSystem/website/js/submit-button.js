
// IDの確定ボタン押下時
document.getElementById('update_all').onclick = function ()
{
    // IDを入力して確定した場合の挙動
    // 入力値を受け取る
    var input_id_sug = Number(document.getElementById("input_id").value);
    var input_pattern = /^([1-9]\d*|0)$/;

    // 値のチェック
    if (input_id_sug != "" && input_pattern.test(input_id_sug) && (100000000000000 <= input_id_sug) && (input_id_sug <= 999999999999999))
    {
        // IDを表示する
        document.getElementById("input_id").value = input_id_sug;
        // 値を表示する
        document.getElementById("show_id_text").innerText = input_id_sug;
        // 値の更新を依頼する
        update_data_by_device(input_id_sug);
    }
    else
    {
        alert('Error: Please enter the correct 15-digit Device_id');
        // 値を表示する
        document.getElementById("show_id_text").innerText = 'Error';
    }
    
};
// IDをリストから選んだ時
function device_id_selected() {
    var calling_obj = event.target;
    // 呼び出したIDを表示する
    document.getElementById("input_id").value = calling_obj.id;
    document.getElementById("show_id_text").innerText = calling_obj.id;

    // 値の更新を依頼する
    update_data_by_device(calling_obj.id);
};

// 温度用閾値の確定ボタン押下時
// temp_upper, temp_lowerのjsonへ
document.getElementById('sub_temp_threshold').onsubmit = function ()
{
    // デバイスIDを取得
    var device_id = document.getElementById("input_id").value;
    // 大小関係をチェック
    if (low_temp.value < high_temp.value) {
        // 湿度の情報を追加
        var humi_upper = document.getElementById("high_humi").value;
        var humi_lower = document.getElementById("low_humi").value;
        create_threshold_json(high_temp.value, low_temp.value, humi_upper, humi_lower);
        // メッセージのOKを押している間にデータを送信する
        window.alert("(Temperature)Low" + low_temp.value + "℃,High" + high_temp.value + "℃");
        // リンクで遷移
        window.location.href = "#threshold_select";
    }
    else {
        alert('(Temperature)Error: Wrong size of threshold value');    
    }

    // リンクで遷移
    window.location.href = "#threshold_select";
};

// 湿度用閾値の確定ボタン押下時
//humi_upper, humi_lowerのjsonへ
document.getElementById('sub_humi_threshold').onsubmit = function ()
{
    // デバイスIDを取得
    var device_id = document.getElementById("input_id").value;
    // 大小関係をチェック
    if (low_humi.value < high_humi.value) {
        if ((0 <= low_humi.value) && (low_humi.value < 100) && (0 < high_humi.value) && (high_humi.value <= 100)) {
            // 温度の情報を追加
            var temp_upper = document.getElementById("high_temp").value;
            var temp_lower = document.getElementById("low_temp").value;
            create_threshold_json(temp_upper, temp_lower, high_humi.value, low_humi.value);
            // メッセージのOKを押している間にデータを送信する
            window.alert("(Humidity)Low" + low_humi.value + "%,High" + high_humi.value + "%");
            // リンクで遷移
            window.location.href = "#threshold_select";
        }
        else {
            alert('(Humidity)Error: Set the threshold in the range of 0-100%');
        }
    }
    else {
        alert('(Humidity)Error: Wrong size of threshold value');
    }
    // 送信後に新データを受信する
    ref_threshold(device_id);
    // リンクで遷移
    window.location.href = "#threshold_select";
};

// 閾値をJSONに変換する
function create_threshold_json(temp_upper, temp_lower, humi_upper, humi_lower)
{
    // 閾値の組を作る
    var threshold_data = {};
    threshold_data["temp_upper"] = temp_upper;
    threshold_data["temp_lower"] = temp_lower;
    threshold_data["humi_upper"] = humi_upper;
    threshold_data["humi_lower"] = humi_lower;
    // 閾値のJSONを作る
    var threshold_json = JSON.stringify(threshold_data);
    threshold_json = JSON.parse(threshold_json);

    // デバイスIDを取得
    var device_id = document.getElementById("input_id").value;
    // 閾値を送信する関数を呼び出す
    set_threshold(device_id, threshold_json);
};

// プロットデータのうち、最新のデータのみを取得する処理
function getLatestSensorInfo(device_id)
{
    // APIサーバのURL
    const api_url = "https://breeding-support-syste.ml:8080/getLatestSensorInfo/";
    const subscription_json_data = "testdata";
    const obj = { device_id: device_id, user_data: subscription_json_data };
    const body_data = Object.keys(obj).map((key) => key + "=" + encodeURIComponent(obj[key])).join("&");

    // APIサーバからデータの取得
    fetch(api_url, {
        method: 'POST',
        mode: 'cors',
        cache: 'no-cache',
        headers: new Headers({
            'Content-Type': 'application/x-www-form-urlencoded; charset=utf-8'
        }),
        body: body_data,
    }).then(response => {

        // 通信エラー
        if (!response.ok) {
            alert("通信エラーが発生しました、ネットワーク接続を確認してください")
        }
        else {
            return response.json();
            // テキストとして渡すreturn response.text();
        }

    }).then(recv_json => {
        // 値を表示する
        document.getElementById("show_latest_temp").innerText = recv_json.temp + '℃';
        document.getElementById("show_latest_humi").innerText = recv_json.humi + '%';
    })
        .catch(error_message => {
            alert("getLatestSensorInfo" + error_message);
        });
}

// IDが更新された場合の表示更新処理
function update_data_by_device(device_id)
{
    // IDを渡しつつ現在の気温湿度を表示
    getLatestSensorInfo(device_id);
    // 閾値を取得する
    ref_threshold(device_id);
    // IDを渡して通知の表示
    getAPINotification(device_id);
    // IDを渡しつつグラフの表示
    click_scale();
};