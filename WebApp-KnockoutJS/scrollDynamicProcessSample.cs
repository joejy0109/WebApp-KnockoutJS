var posTop = -1,
    height = 0,
    curPosTop = 0,
    idx1 = 0,
    delay = 500;

var dynamicGraph = function () {                
    var $win = $(window);
    if ($win.scrollTop() <= posTop) return; // 역 스크롤 bottom -> top 시 작업 종료 (단방향 동작 top -> bottom)
    height = $win.height(); 
    posTop = $win.scrollTop();
    curPosTop = height + posTop; // 현재 top position               
    var totalHeight = $('html').prop('scrollHeight');
    if (curPosTop >= totalHeight) {
        return;// 현재 위치(top)이 마지막에 도달하면 작업 종료
    }
    
    // 대상 그래프 렌더링                
    $("div.gray:eq(" + idx1 + ")").each(function () {
        if (curPosTop <= $(this).offset().top) {
            return true;
        }
        var $assign = $(".assign:eq(" + idx1 + ")");
        var s = $assign.data("size");
        var w = $(this).data("ideal");
        $assign.animate({ width: s }, delay);
        $(this).animate({ width: w > 0 ? '71%' : '0%' }, delay);
        idx1++;

        //console.log(idx1);
    });
};

// 반복 초기화 호출부
function execute() {
    posTop = -1;
    height = 0;
    curPosTop = 0;
    idx1 = 0;
    dynamicGraph();
}

// 스크롤바 이벤트 초기화
$(window).on('scroll', dynamicGraph);
