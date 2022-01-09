// Webページが最初にロードされたとき実行される(初期値に関する処理など)
function ref_threshold(device_id)
{
    // APIサーバのURL
    const api_url = "https://breeding-support-syste.ml:8080/getJsonConfig/";
    const obj = { device_id: device_id};
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
            // テキストとして渡す
            //return response.text();
        }

        }).then(recv_json => {
        // 値を表示する
        document.getElementById("low_temp").value = recv_json.temp_lower;
        document.getElementById("high_temp").value = recv_json.temp_upper;
        document.getElementById("low_humi").value = recv_json.humi_lower;
        document.getElementById("high_humi").value = recv_json.humi_upper;
    })
        .catch(error_message => {
            alert("ref_threshold: " + error_message);
        });
};

// JSON形式で閾値をサーバに送信する
function set_threshold(device_id, threshold_json)
{
    // APIサーバのURL
    const api_url = "https://breeding-support-syste.ml:8080/setJsonConfig/";
    const obj = {
        device_id: device_id, temp_upper: threshold_json.temp_upper, temp_lower: threshold_json.temp_lower,
        humi_upper: threshold_json.humi_upper, humi_lower: threshold_json.humi_lower};
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
            // テキストとして渡す
            return response.text();
        }

    }).then(recv_json => {
        // 値を判定する
        if (recv_json == '0') {
            alert("閾値の更新に成功しました");
        }
        else {
            alert("閾値の更新に失敗しました")
        }
        
    })
        .catch(error_message => {
            alert("set_threshold: " + error_message);
        });
};