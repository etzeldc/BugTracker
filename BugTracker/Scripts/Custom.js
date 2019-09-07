$(".userTable").DataTable({
    'paging': true,
    'lengthMenu': [[5, 10, 25, -1], [5, 10, 25, "All"]],
    'searching': true,
    'ordering': true,
    'info': true,
    'autoWidth': false,
});

$(function () {
    $(".comCan").hide();

    $(".comEdit").click(function () {
        $("a.comEdit").hide();
        $("a.comCan").show();
    })
    $(".comCan").click(function () {
        $("a.comEdit").show();
        $("a.comCan").hide();
    })
})