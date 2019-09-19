namespace CrispyWaffle.Notification
{
    using Attributes;

    /// <summary>
    /// Enum NotificationType
    /// </summary>
    public enum MailNotificationType
    {

        /// <summary>
        /// The none
        /// </summary>
        [HumanReadable("Test")]
        [InternalValue("ExceptionFormatter")]
        NONE,

        /// <summary>
        /// The error
        /// </summary>
        [HumanReadable("Notificação de erro")]
        [InternalValue("ExceptionFormatter")]
        ERROR,

        /// <summary>
        /// The photo dimension
        /// </summary>
        [HumanReadable("Foto do produto com dimensões incorretas")]
        [InternalValue("PhotoDimensionTemplate")]
        PHOTO_DIMENSION,

        /// <summary>
        /// The no photo
        /// </summary>
        [HumanReadable("Produto sem foto")]
        [InternalValue("NoPhotoTemplate")]
        NO_PHOTO,

        /// <summary>
        /// The NCM
        /// </summary>
        [HumanReadable("NCM inválido")]
        [InternalValue("NcmTemplate")]
        NCM,

        /// <summary>
        /// The seller
        /// </summary>
        [HumanReadable("Vendedor televendas não localizado no Sankhya")]
        [InternalValue("SellerNotFoundTemplate")]
        SELLER,

        /// <summary>
        /// The uses grid
        /// </summary>
        [HumanReadable("Produto usa grade inválido")]
        [InternalValue("ProductUsesGridInvalidTemplate")]
        USES_GRID,

        /// <summary>
        /// The duplicate course episode
        /// </summary>
        [HumanReadable("Curso online com episódio duplicado")]
        [InternalValue("DuplicateCourseEpisodeTemplate")]
        DUPLICATED_COURSE_EPISODE,

        /// <summary>
        /// The invalid episode link
        /// </summary>
        [HumanReadable("Curso online com episódio inválido")]
        [InternalValue("InvalidEpisodeLinkTemplate")]
        INVALID_EPISODE_LINK,

        /// <summary>
        /// The missing specification
        /// </summary>
        [HumanReadable("Produto/SKU sem especificação")]
        [InternalValue("ProductMissingSpecificationTemplate")]
        MISSING_SPECIFICATION,

        /// <summary>
        /// The invalid subscription
        /// </summary>
        [HumanReadable("Assinatura inválida")]
        [InternalValue("InvalidSubscriptionTemplate")]
        INVALID_SUBSCRIPTION,

        /// <summary>
        /// The ean repeated
        /// </summary>
        [HumanReadable("EAN repetido para diversos produtos")]
        [InternalValue("EanRepeatedTemplate")]
        EAN_REPEATED
    }
}
