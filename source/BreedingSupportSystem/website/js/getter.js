function get_selected_device_id()
{
    // 現在選択中のIDを取得する
    var current_device_id = document.getElementById("input_id").value;
    if (current_device_id === "" || current_device_id === null || current_device_id === "undefined")
    {
        // 空文字、null、underfinedの場合は-1を返す
        return -1;
    }
    else
    {
        // 現在選択中のIDを返却する
        return current_device_id;
    }
};