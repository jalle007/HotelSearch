$(document).on({
  ajaxStart: function () {
    console.log("ajax");
    $("body").addClass("loading");
  },
  ajaxStop: function () {
    console.log("ajaxStop");
    $("body").removeClass("loading");
  }
});

$(document).ready(function () {
   
  $("#checkIn").datepicker({ minDate: new Date() });
  $("#checkOut").datepicker({ minDate: new Date() });

  // this is the id of the form
  $("#searchForm").submit(function (e) {
    console.log("submit");
    e.preventDefault(); // avoid to execute the actual submit of the form.

    var form = $("#searchForm"); // $(this);
    var url = form.attr('action');
    var data = form.serialize();
    console.log("data ", data);

    $.ajax({
      type: "POST",
      url: url,
      data: data,// serializes the form's elements.
      //cache: false,
      //processData: false,
      //contentType: false,
      success: function (view) {
        console.log(view); // show response from the php script.

        $('#result').html(view);
      }
    });


  });

  $("#city").keyup(function () {
    var val = $("#city").val();
    var len = val.length;

    if (len > 2) {
      //search
      search(val);
    }

    //console.log(val);
  });

  function search(keyword) {
    $.ajax({
      type: "POST",
      url: 'Home/GetLocation?keyword=' + keyword,
      //dataType: 'json',
      //contentType: "application/json; charset=utf-8",

      success: function (data) {
        console.log("data ", data);
        //debugger;
        //var dataP = $.parseJSON(data);
        var c = data.map(lang => lang.city);
        //console.log("c ", c);

        $("#city").autocomplete({
          minLength: 2,
          source: function (request, response) {
            //data :: JSON list defined
            response($.map(data, function (value, key) {
              return {
                label: value.city,
                value: value.cityCode
              }
            }));
          },

          select: function (event, ui) {
            console.log('ui', ui.item);
            $("#city").val(ui.item.label);
            $("#cityCode").val(ui.item.value);
            return false;
          }

        });
      },
      error:
        function (error) {
          console.log("error", error);
        }
    });
  }

});