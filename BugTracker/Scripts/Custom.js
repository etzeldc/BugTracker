﻿$(".userTable").DataTable({
    'paging': true,
    'lengthMenu': [[5, 10, 25, -1], [5, 10, 25, "All"]],
    'searching': true,
    'ordering': true,
    'info': true,
    'autoWidth': false,
});

//$(".skrollDown").sccrollTop($(".skrollDown")[0].scrollHeight);