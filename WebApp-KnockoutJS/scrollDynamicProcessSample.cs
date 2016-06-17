var posTop = -1,
    height = 0,
    curPosTop = 0,
    idx1 = 0,
    idx2 = 0,
    delay = 500;

var proc;
var drawGraph = function () {
    var $win = $(window);
    if ($win.scrollTop() <= posTop) return;
    height = $win.height();
    posTop = $win.scrollTop();
    curPosTop = height + posTop - 84;
    
    var totalHeight = $('html').prop('scrollHeight') - 200;

    if (curPosTop >= (totalHeight-200)) {
        if (proc == null) {                        
            proc = setTimeout(function () {
                proc = null;
            }, 1000);
            console.log('loading...' + ' == ' + curPosTop + " / " + totalHeight);
        }
        return;
    }

    //console.log('scrollHeight: ' + totalHeight + ' /postTop-height: ' + (curPosTop - height) + ' /posTop: ' + curPosTop);
   
   // scroll 이벤트 대상 item
    $("div.gray:eq(" + idx1 + ")").each(function () {
        if (curPosTop <= $(this).offset().top)
            return true;
        
        // 병행 처리할 item
        var $assign = $(".assign:eq(" + idx1 + ")");

        var s = $assign.data("size");
        var w = $(this).data("ideal");                   

        $assign.animate({ width: s }, delay);
        $(this).animate({ width: w > 0 ? '70%' : '0%' }, delay);

        //console.log(idx1);

        idx1++; // 순서대로 처리되어진다 (전제조건: up scroll시 이벤트 발생되지 않는 조건)
    });

    //$(".assign:eq(" + idx1 + ")").each(function () {
    //    if (curPosTop <= $(this).offset().top)
    //        return true;
    //    console.log(idx1);
    //    var s = $(this).data("size");
    //    $(this).animate({ width: s }, 1000);

    //    idx1++;
    //});

    //$("div.gray:eq(" + idx2 + ")").each(function () {
    //    if (curPosTop <= ($(this).offset().top))
    //        return true;

    //    console.log(idx2);

    //    var s = $(this).data("ideal");
    //    $(this).animate({ width: (s > 0 ? '70%' : '0%') }, 1000);

    //    idx2++;
    //});
};
