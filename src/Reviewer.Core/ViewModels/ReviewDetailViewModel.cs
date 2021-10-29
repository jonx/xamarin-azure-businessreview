using System;
using Reviewer.SharedModels;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Windows.Input;
namespace Reviewer.Core
{
    public class ReviewDetailViewModel : BaseViewModel
    {
        Review review;
        public Review Review { get => review; set => SetProperty(ref review, value); }

        Business business;
        public Business Business { get => business; set => SetProperty(ref business, value); }

        bool editable;
        public bool Editable { get => editable; set => SetProperty(ref editable, value); }

        public ReviewDetailViewModel(Review review, Business business)
        {
            Review = review;
            Business = business;

            Title = "Details";

            var idService = DependencyService.Get<IMicrosoftAuthService>();

            Task.Run(async () =>
            {
                var user = await idService.OnSignInAsync();

                if (user?.Id == Review.AuthorId)
                    Editable = true;
                else
                    Editable = false;
            });
        }
    }
}
