$(".userTable").DataTable({
    'paging': true,
    'lengthMenu': [[5, 10, 25, -1], [5, 10, 25, "All"]],
    'searching': true,
    'ordering': true,
    'info': true,
    'autoWidth': false,
});



//$("#skrollDown").click(function(){
//    var mySkroll = $("#invisible");
//    mySkroll.scrollTop(myscroll.get(0).scrollHeight);
//});
    //scrollTop: $(".skrollDown").get(0).scrollHeight


//$(document).ready(function () {
//    $("#down").animate({
//        $("#down").scrollHeight
//    }, 100);
//})

//$("#here").click(function () {
//    scrollTop: $("#down").get(0).scrollHeight
//}, 2000);

//$(document).ready(function () {
//    $(".ticketBtn").on("click", function () {
//        $.ajax({
//            url: '@Url.Action("Edit", "Tickets")',
//            type: "POST",
//        }).done(function(result) {
//            $("#tix").html(result);
//        })
//    });
//});

