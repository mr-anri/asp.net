$(function copy_click() {
    $("#copy").on("click", function () {

        var array_of_id_role_permisiions = [];

        $("input:checked").each(function () {
            if ($(this).checked) {
                array_of_id_role_permisiions.push($(this).attr('id'))
            }
        })
        //$('input:checkbox:checked').each(function () {
        //    array_of_id_role_permisiions.push($(this).attr('id'))
        //});

        //alert(checked);
        //location.href = "Index";
        //$.get('Index');
 

        $.ajax({
            url: "Get_list_of_permission_into_role", //зд вызывается метод котроллера. те папка, имя котроллера, метод
            type: "post",
            dataType: "json",
            crossDomain: true,
            contentType: "application/json",


            data: JSON.stringify({ //приводим к типа жейсон
                Id: array_of_id_role_permisiions, // на сервер отправка данных полей имена и телефонов
            }),

            success: function (response) {
                var resultDiv = $("#results");
                for (var i = 0; i < response.length; i++)
                {
                    resultDiv.append(response[i].Id_ist + ", " + response[i].Id_naz + " " + response[i].PermissionStringValue + "<br/>");
                }

                //alert(resultDiv);
            },
            error: function (XMLHttpRequest /*:jqXHR*/, textStatus, errorThrown) {
                console.log(XMLHttpRequest,errorThrown, textStatus);
            }
        });


    });
});